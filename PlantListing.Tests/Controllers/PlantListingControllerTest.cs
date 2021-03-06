using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PlantListing.Controllers;
using PlantListing.Images;
using PlantListing.Infrastructure;
using PlantListing.Models;
using PlantListing.ViewModels;
using Xunit;
using Microsoft.Extensions.Logging;

namespace PlantListing.Test
{
    public class PlantListingControllerTest
    {
        private readonly DbContextOptions<PlantListingContext> _dbOptions;
        private readonly DbContextOptions<PlantListingContext> _dbOptions_Delete;
        private Mock<IPlantImageService> _mockPlantImageService;

        public PlantListingControllerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<PlantListingContext>()
                .UseInMemoryDatabase(databaseName: "in-memory plant details")
                .Options;

            SeedDBContext(_dbOptions);

            _dbOptions_Delete = new DbContextOptionsBuilder<PlantListingContext>()
                .UseInMemoryDatabase(databaseName: "in-memory plant details for delete")
                .Options;

            SeedDBContext(_dbOptions_Delete);

            _mockPlantImageService = new Mock<IPlantImageService>();
            _mockPlantImageService.Setup(c => c.GetPlantImageUri(It.IsAny<string>())).Returns("");
        }       

        private void SeedDBContext(DbContextOptions<PlantListingContext> dbOptions)
        {
            using (var dbContext = new PlantListingContext(dbOptions))
            {
                if (!dbContext.PlantCategories.Any())
                {
                    dbContext.PlantCategories.AddRange(PlantListingContextSeed.GetPreconfiguredPlantCategories());
                }

                if (!dbContext.WeightUnits.Any())
                {
                    dbContext.WeightUnits.AddRange(PlantListingContextSeed.GetPreconfiguredWeightUnits());
                }

                if (!dbContext.PlantDetails.Any())
                {
                    dbContext.PlantDetails.AddRange(PlantListingContextSeed.GetPreconfiguredPlantDetails());
                }

                dbContext.SaveChanges();
            }
        }

        #region GetPlantListing
        [Theory]
        [InlineData(10, 0, 8)]
        [InlineData(5, 0, 5)] // test pagination
        [InlineData(5, 1, 3)] // test pagination
        [InlineData(5, 2, 0)] // test pagination
        public async Task Get_plant_listing_success(int pageSize, int pageIndex, int expectedCount)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetPlantListing(pageSize, pageIndex);

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>>(actionResult);
            Assert.Equal(expectedCount, actionResult.Value.Data.Count());
        }
        #endregion

        #region GetPlantListingByPlantDetailsIds
        [Theory]
        [InlineData(null, 5, 0)]
        [InlineData("", 5, 0)]
        [InlineData("          ", 5, 0)]
        [InlineData("1,2,3", 5, 0)]
        [InlineData("1a|2b|3c", 5, 0)]
        [InlineData("1|2|3|", 5, 0)]
        public async Task Get_plant_listing_by_plantDetailsIds_bad_request_response(string plantDetailsIds, int pageSize, int pageIndex)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetPlantListingByPlantDetailsIds(plantDetailsIds, pageSize, pageIndex);

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>>(actionResult);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Theory]
        [InlineData("1|2|3", 5, 0, 3)]
        [InlineData("1|1|1", 5, 0, 1)]
        [InlineData("1 | 2 | 3", 5, 0, 3)]
        [InlineData("1|2|3", 2, 0, 2)] // test pagination
        [InlineData("1|2|3", 2, 1, 1)] // test pagination
        public async Task Get_plant_listing_by_plantDetailsIds_success(string plantDetailsIds, int pageSize, int pageIndex, int expectedCount)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetPlantListingByPlantDetailsIds(plantDetailsIds, pageSize, pageIndex);

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>>(actionResult);
            Assert.Equal(expectedCount, actionResult.Value.Data.Count());
        }
        #endregion

        #region GetPlantListingByProducerId
        [Theory]
        [InlineData("cktan", 5, 0, 2)]
        [InlineData("mgkoh", 5, 0, 1)]
        [InlineData("user0001", 5, 0, 1)]
        [InlineData("wpkeoh", 5, 0, 4)]
        [InlineData("cktan", 1, 0, 1)] // test pagination
        [InlineData("wpkeoh", 2, 1, 2)] // test pagination
        [InlineData("unknown", 5, 0, 0)]
        public async Task Get_plant_listing_by_producerId_success(string userId, int pageSize, int pageIndex, int expectedCount)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetMyPlantListing(new GetMyPlantListingViewModel()
            {
                UserId = userId,
                PageSize = pageSize,
                PageIndex = pageIndex
            });;

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>>(actionResult);
            Assert.Equal(expectedCount, actionResult.Value.Data.Count());
        }
        #endregion

        #region GetPlantListingByCategoryId
        [Theory]
        [InlineData(null, 10, 0, 8)]
        [InlineData(null, 5, 0, 5)] // test pagination
        [InlineData(null, 5, 1, 3)] // test pagination
        [InlineData(null, 5, 2, 0)] // test pagination
        [InlineData(1, 5, 0, 1)]
        [InlineData(5, 5, 0, 4)]
        [InlineData(4, 5, 0, 0)]
        [InlineData(99, 5, 0, 0)]
        public async Task Get_plant_listing_by_categoryId_success(int? categoryId, int pageSize, int pageIndex, int expectedCount)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetPlantListingByCategoryId(categoryId, pageSize, pageIndex);

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>>(actionResult);
            Assert.Equal(expectedCount, actionResult.Value.Data.Count());
        }
        #endregion

        #region SearchPlantListing
        [Fact]
        public async Task Search_plant_listing_bad_request_response()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.SearchPlantListing(null);

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>>(actionResult);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Theory]
        [InlineData("green", null, 5, 0, 3)]
        [InlineData("green      ", null, 5, 0, 3)] // trailing spaces in keyword
        [InlineData("       green", null, 5, 0, 3)] // leading spaces in keyword
        [InlineData("       green        ", null, 5, 0, 3)] // trailing & leading spaces in keyword
        [InlineData("green", null, 2, 0, 2)] // test pagination
        [InlineData("green", null, 2, 1, 1)] // test pagination
        [InlineData("green", null, 5, 10, 0)] // test pagination
        [InlineData("green", 1, 5, 0, 1)] // test category
        [InlineData("green", 2, 5, 0, 1)] // test category
        [InlineData("green", 5, 5, 0, 1)] // test category
        [InlineData("green", 3, 5, 0, 0)] // test category
        [InlineData("milk tea", null, 5, 0, 0)]
        public async Task Search_plant_listing_success(string keyword, int? categoryId, int pageSize, int pageIndex, int expectedCount)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.SearchPlantListing(keyword, categoryId, pageSize, pageIndex);

            //Assert
            Assert.IsType<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>>(actionResult);
            Assert.Equal(expectedCount, actionResult.Value.Data.Count());
        }
        #endregion

        #region GetPlantDetails
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task Get_plant_details_bad_request_response(long plantDetailsId)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetPlantDetails(plantDetailsId);

            //Assert
            Assert.IsType<ActionResult<PlantDetailsViewModel>>(actionResult);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task Get_plant_details_not_found_response()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);
            var plantDetailsId = 999;

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetPlantDetails(plantDetailsId);

            //Assert
            Assert.IsType<ActionResult<PlantDetailsViewModel>>(actionResult);
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task Get_plant_details_success()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            var plantDetailsId = 3;
            var expectedName = "Japanese Cucumber";
            var expectedDescription = "Green color fruit";
            var expectedCategory = "Fruit";
            var expectedPrice = 1.00m;
            var expectedWeight = 500.0m;
            var expectedUnit = "g";
            var expectedStock = 50;
            var expectedUserId = "mgkoh";

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.GetPlantDetails(plantDetailsId);

            //Assert
            Assert.IsType<ActionResult<PlantDetailsViewModel>>(actionResult);
            Assert.Equal(plantDetailsId, actionResult.Value.PlantDetailsId);
            Assert.Equal(expectedName, actionResult.Value.Name);
            Assert.Equal(expectedDescription, actionResult.Value.Description);
            Assert.Equal(expectedCategory, actionResult.Value.Category);
            Assert.Equal(expectedPrice, actionResult.Value.Price);
            Assert.Equal(expectedWeight, actionResult.Value.Weight);
            Assert.Equal(expectedUnit, actionResult.Value.Unit);
            Assert.Equal(expectedStock, actionResult.Value.Stock);
            Assert.Equal(expectedUserId, actionResult.Value.UserId);
        }
        #endregion

        #region UpdatePlantDetails
        [Fact]
        public async Task Update_plant_details_not_found_reponse()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            var userId = "cktan";
            var NotFoundUpdatePlantDetailsViewModel = new CreateEditPlantDetailsViewModel()
            {
                PlantDetailsId = 99,
                Name = "Bean sprouts",
                Description = "Organic bean sprouts",
                Category = "Vegetable",
                Price = 0.80m,
                Weight = 200.0m,
                Unit = "g",
                Stock = 100,
                UserId = userId
            };

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.UpdatePlantDetails(NotFoundUpdatePlantDetailsViewModel);

            //Assert
            Assert.IsType<NotFoundObjectResult>(actionResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("cktan")]
        public async Task Update_plant_details_unauthorized_reponse(string userId)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            var updatePlantDetailsViewModel = new CreateEditPlantDetailsViewModel()
            {
                PlantDetailsId = 5,
                Name = "Fragrant Garlic",
                Description = "Fragrant Garlic (Small)",
                Category = "Spice",
                Price = 1.50m,
                Weight = 100.0m,
                Unit = "g",
                Stock = 100,
                UserId = userId
            };

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext,_mockPlantImageService.Object);
            var actionResult = await plantDetailsController.UpdatePlantDetails(updatePlantDetailsViewModel);

            //Assert
            Assert.IsType<UnauthorizedResult>(actionResult);
        }

        [Theory]
        [InlineData("", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "g", 100)] // name
        [InlineData(null, "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "g", 100)]
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "", 1.50, 100.0, "g", 100)] // category
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", null, 1.50, 100.0, "g", 100)]
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", " spice", 1.50, 100.0, "g", 100)]
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", -1.50, 100.0, "g", 100)] // price
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 0, "g", 100)] // weight
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, -100, "g", 100)]
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "", 100)] // unit
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, null, 100)]
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "mg", 100)]
        [InlineData("Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "g", -1)] // stock
        public async Task Update_plant_details_bad_request_response(string name, string description, string category, decimal price, decimal weight, string unit, int stock)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            var userId = "wpkeoh";
            var invalidUpdatePlantDetailsViewModel = new CreateEditPlantDetailsViewModel()
            {
                PlantDetailsId = 5,
                Name = name,
                Description = description,
                Category = category,
                Price = price,
                Weight = weight,
                Unit = unit,
                Stock = stock,
                UserId = userId
            };

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.UpdatePlantDetails(invalidUpdatePlantDetailsViewModel);

            //Assert
            Assert.IsType<BadRequestResult>(actionResult);
        }

        [Fact]
        public async Task Update_plant_details_success()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            var userId = "wpkeoh";
            var updatePlantDetailsViewModel = new CreateEditPlantDetailsViewModel()
            {
                PlantDetailsId = 5,
                Name = "Fragrant Garlic",
                Description = "Fragrant Garlic (Small)",
                Category = "Spice",
                Price = 1.50m,
                Weight = 100.0m,
                Unit = "g",
                Stock = 100,
                UserId = userId
            };

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.UpdatePlantDetails(updatePlantDetailsViewModel);

            //Assert
            Assert.IsType<NoContentResult>(actionResult);
        }
        #endregion

        #region CreatePlantDetails       
        [Theory]
        [InlineData(null, "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "g", 100)]
        [InlineData("cktan", "", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "g", 100)] // name
        [InlineData("cktan", null, "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "g", 100)]
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "", 1.50, 100.0, "g", 100)] // category
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", null, 1.50, 100.0, "g", 100)]
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", " spice", 1.50, 100.0, "g", 100)]
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", -1.50, 100.0, "g", 100)] // price
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 0, "g", 100)] // weight
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, -100, "g", 100)]
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "", 100)] // unit
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, null, 100)]
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "mg", 100)]
        [InlineData("cktan", "Fragrant Garlic", "Fragrant Garlic (Small)", "Spice", 1.50, 100.0, "g", -1)] // stock
        public async Task Create_plant_details_bad_request_reponse(string userId, string name, string description, string category, decimal price, decimal weight, string unit, int stock)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            var invalidCreatePlantDetailsViewModel = new CreateEditPlantDetailsViewModel()
            {
                PlantDetailsId = 0,
                Name = name,
                Description = description,
                Category = category,
                Price = price,
                Weight = weight,
                Unit = unit,
                Stock = stock,
                UserId = userId
            };

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.CreatePlantDetails(invalidCreatePlantDetailsViewModel);

            //Assert
            Assert.IsType<BadRequestResult>(actionResult.Result);
        }

        [Fact]
        public async Task Create_plant_details_success()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions);

            var userId = "cktan";
            var validCreatePlantDetailsViewModel = new CreateEditPlantDetailsViewModel()
            {
                Name = "Bean sprouts",
                Description = "Organic bean sprouts",
                Category = "Vegetable",
                Price = 0.80m,
                Weight = 100.0m,
                Unit = "g",
                Stock = 100,
                UserId = userId
            };

            var expectedPlantDetailsId = 9;
            var expectedName = "Bean sprouts";
            var expectedDescription = "Organic bean sprouts";
            var expectedCategory = "Vegetable";
            var expectedPrice = 0.80m;
            var expectedWeight = 100.0m;
            var expectedUnit = "g";
            var expectedStock = 100;
            var expectedUserId = "cktan";

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.CreatePlantDetails(validCreatePlantDetailsViewModel);

            //Assert
            Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal("GetPlantDetails", (actionResult.Result as CreatedAtActionResult).ActionName);
            Assert.IsType<PlantDetailsViewModel>((actionResult.Result as CreatedAtActionResult).Value);

            PlantDetailsViewModel actualViewModel = (actionResult.Result as CreatedAtActionResult).Value as PlantDetailsViewModel;
            Assert.Equal(expectedPlantDetailsId, actualViewModel.PlantDetailsId);
            Assert.Equal(expectedName, actualViewModel.Name);
            Assert.Equal(expectedDescription, actualViewModel.Description);
            Assert.Equal(expectedCategory, actualViewModel.Category);
            Assert.Equal(expectedPrice, actualViewModel.Price);
            Assert.Equal(expectedWeight, actualViewModel.Weight);
            Assert.Equal(expectedUnit, actualViewModel.Unit);
            Assert.Equal(expectedStock, actualViewModel.Stock);
            Assert.Equal(expectedUserId, actualViewModel.UserId);
        }
        #endregion

        #region DeletePlantDetails
        [Fact]
        public async Task Delete_plant_details_not_found_reponse()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions_Delete);

            var plantDetailsId = 99;
            var userId = "cktan";

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.DeletePlantDetails(new DeletePlantDetailsViewModel()
            { 
                UserId = userId,
                PlantDetailsId = plantDetailsId
            });

            //Assert
            Assert.IsType<NotFoundObjectResult>(actionResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("cktan")]
        public async Task Delete_plant_details_unauthorized_reponse(string userId)
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions_Delete);

            var plantDetailsId = 7;

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.DeletePlantDetails(new DeletePlantDetailsViewModel()
            {
                UserId = userId,
                PlantDetailsId = plantDetailsId
            });

            //Assert
            Assert.IsType<UnauthorizedResult>(actionResult);
        }

        [Fact]
        public async Task Delete_plant_details_success()
        {
            //Arrange
            var plantDetailsContext = new PlantListingContext(_dbOptions_Delete);

            var plantDetailsId = 8;
            var userId = "wpkeoh";

            //Act
            var plantDetailsController = new PlantListingController(plantDetailsContext, _mockPlantImageService.Object);
            var actionResult = await plantDetailsController.DeletePlantDetails(new DeletePlantDetailsViewModel()
            {
                UserId = userId,
                PlantDetailsId = plantDetailsId
            });

            //Assert
            Assert.IsType<NoContentResult>(actionResult);
        }
        #endregion      
    }
}
