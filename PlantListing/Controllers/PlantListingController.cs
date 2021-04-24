﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantListing.Infrastructure;
using PlantListing.Models;
using PlantListing.ViewModels;
using Microsoft.AspNetCore.Identity;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using Microsoft.Extensions.Configuration;
using PlantListing.Extensions;
using PlantListing.Images;
using PlantListing.Infrastructure.Exceptions;

namespace PlantListing.Controllers
{
    //[Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PlantListingController : ControllerBase
    {
        private readonly PlantListingContext _context;
        private readonly IPlantImageService _plantImageService;
        private readonly long? _producerId;


        //public PlantListingController(PlantListingContext context)
        //{
        //    _context = context ?? throw new ArgumentNullException(nameof(context));
        //}

        //public PlantListingController(PlantListingContext context, long? producerId = null)
        //{
        //    _context = context ?? throw new ArgumentNullException(nameof(context));
        //    _producerId = producerId;
        //}

        public PlantListingController(PlantListingContext context, IPlantImageService plantImageService, long? producerId = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _plantImageService = plantImageService ?? throw new ArgumentNullException(nameof(plantImageService));
            _producerId = producerId;
        }

        // GET: api/v1/PlantListing/[?pageSize=5&pageIndex=10]
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<PlantDetailsViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>> GetPlantListing([FromQuery] int pageSize = 5, [FromQuery] int pageIndex = 0)
        {
            var root = (IQueryable<PlantDetails>)_context.PlantDetails;

            var totalItems = await root
                .LongCountAsync();

            var itemsOnPage = await root
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedItemsViewModel<PlantDetailsViewModel>(pageIndex, pageSize, totalItems, MapToViewModels(itemsOnPage));
        }

        // GET: api/v1/PlantListing/{producerId}
        [HttpGet]
        [Route("{producerId:long?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<PlantDetailsViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>> GetPlantListingByProducerId(long producerId, [FromQuery] int pageSize = 5, [FromQuery] int pageIndex = 0)
        {
            var root = (IQueryable<PlantDetails>)_context.PlantDetails;
            root = root.Where(d => d.ProducerId == producerId);

            var totalItems = await root
                .LongCountAsync();

            var itemsOnPage = await root
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedItemsViewModel<PlantDetailsViewModel>(pageIndex, pageSize, totalItems, MapToViewModels(itemsOnPage));
        }

        // GET: api/v1/PlantListing/Category/{categoryId}
        [HttpGet]
        [Route("Category/{categoryId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<PlantDetailsViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>> GetPlantListingByCategoryId(int? categoryId, [FromQuery] int pageSize = 5, [FromQuery] int pageIndex = 0)
        {
            var root = (IQueryable<PlantDetails>)_context.PlantDetails;
            if (categoryId.HasValue)
            {
                root = root.Where(d => d.CategoryId == categoryId.Value);
            }

            var totalItems = await root
                .LongCountAsync();

            var itemsOnPage = await root
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedItemsViewModel<PlantDetailsViewModel>(pageIndex, pageSize, totalItems, MapToViewModels(itemsOnPage));
        }

        // GET: api/v1/PlantListing/Search/keyword[?categoryId=1&pageSize=5&pageIndex=10]
        [HttpGet]
        [Route("Search/{keyword:minlength(1)}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<PlantDetailsViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<PlantDetailsViewModel>>> SearchPlantListing(string keyword, [FromQuery] int? categoryId = null, [FromQuery] int pageSize = 5, [FromQuery] int pageIndex = 0)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { Message = $"Keyword should not be empty" });
            }

            var trimmedKeyword = keyword.Trim();

            var root = (IQueryable<PlantDetails>)_context.PlantDetails;
            root = root.Where(d => EF.Functions.Like(d.Name, $"%{trimmedKeyword}%") || EF.Functions.Like(d.Description, $"%{trimmedKeyword}%"));

            if (categoryId.HasValue)
            {
                root = root.Where(d => d.CategoryId == categoryId.Value);
            }

            var totalItems = await root
                .LongCountAsync();

            var itemsOnPage = await root
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedItemsViewModel<PlantDetailsViewModel>(pageIndex, pageSize, totalItems, MapToViewModels(itemsOnPage));
        }


        // GET: api/v1/PlantListing/PlantDetails/{plantDetailsId}
        [HttpGet]
        [Route("PlantDetails/{plantDetailsId:long?}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(PlantDetailsViewModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PlantDetailsViewModel>> GetPlantDetails(long plantDetailsId)
        {
            if (plantDetailsId <= 0)
            {
                return BadRequest(new { Message = $"Invalid plant details id {plantDetailsId}" });
            }

            var plantDetails = await _context.PlantDetails.FindAsync(plantDetailsId);

            if (plantDetails == null)
            {
                return NotFound(new { Message = $"Plant details with id {plantDetailsId} not found." });
            }

            return MapToViewModel(plantDetails);
        }

        // PUT: api/v1/PlantListing/PlantDetails
        [HttpPut]
        [Route("PlantDetails")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdatePlantDetails([FromForm] CreateUpdatePlantDetailsViewModel plantDetailsViewModel)
        {
            var plantDetails = await _context.PlantDetails.FindAsync(plantDetailsViewModel.PlantDetailsId);
            if (plantDetails == null)
            {
                return NotFound(new { Message = $"Plant details with id {plantDetailsViewModel.PlantDetailsId} not found." });
            }

            if (!IsProducer() || plantDetails.ProducerId != GetProducerId())
            {
                return Unauthorized();
            }

            GetChangesFromViewModel(plantDetails, plantDetailsViewModel);
            if (!plantDetails.IsValid() || (plantDetailsViewModel.ImageFile != null && !plantDetailsViewModel.ImageFile.IsValidImage()))
            {
                return BadRequest();
            }

            try
            {
                if (plantDetailsViewModel.ImageFile != null)
                {
                    var currentImageName = plantDetails.ImageName;
                    var plantImageViewModel = await _plantImageService.ReplaceImageAsync(currentImageName, plantDetailsViewModel.ImageFile);
                    plantDetails.ImageName = plantImageViewModel.ImageName;
                }

                _context.Entry(plantDetails).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlantDetailsExists(plantDetailsViewModel.PlantDetailsId))
                {
                    return NotFound(new { Message = $"Plant details with id {plantDetailsViewModel.PlantDetailsId} not found." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/v1/PlantListing/PlantDetails
        [HttpPost]
        [Route("PlantDetails")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(PlantDetailsViewModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PlantDetailsViewModel>> CreatePlantDetails([FromForm] CreateUpdatePlantDetailsViewModel plantDetailsViewModel)
        {
            if (!IsProducer())
            {
                return Unauthorized();
            }

            var plantDetails = new PlantDetails();
            GetChangesFromViewModel(plantDetails, plantDetailsViewModel);
            plantDetails.ProducerId = GetProducerId();

            if (!plantDetails.IsValid() || (plantDetailsViewModel.ImageFile != null && !plantDetailsViewModel.ImageFile.IsValidImage()))
            {
                return BadRequest();
            }

            if (plantDetailsViewModel.ImageFile != null)
            {
                var plantImageViewModel = await _plantImageService.UploadImageAsync(plantDetailsViewModel.ImageFile);
                plantDetails.ImageName = plantImageViewModel.ImageName;
            }
         
            _context.PlantDetails.Add(plantDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlantDetails", new { id = plantDetails.PlantDetailsId }, MapToViewModel(plantDetails));
        }

        // DELETE: api/v1/PlantListing/PlantDetails/{plantDetailsId}
        [HttpDelete]
        [Route("PlantDetails/{plantDetailsId:long?}")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeletePlantDetails(long plantDetailsId)
        {
            var plantDetails = await _context.PlantDetails.FindAsync(plantDetailsId);
            if (plantDetails == null)
            {
                return NotFound(new { Message = $"Plant details with id {plantDetailsId} not found." });
            }

            if (!IsProducer() || plantDetails.ProducerId != GetProducerId())
            {
                return Unauthorized();
            }

            if(!string.IsNullOrEmpty(plantDetails.ImageName))
            {
               _plantImageService.DeleteImageAsync(plantDetails.ImageName); // no need await for this
            }

            _context.PlantDetails.Remove(plantDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlantDetailsExists(long id)
        {
            return _context.PlantDetails.Any(e => e.PlantDetailsId == id);
        }

        private bool IsProducer()
        {
            // TODO
            //throw new NotImplementedException();
            return _producerId.HasValue && _producerId.Value > 0;
        }

        private long GetProducerId()
        {
            // TODO
            //throw new NotImplementedException();
            return _producerId ?? -1;
        }

        private PlantDetailsViewModel MapToViewModel(PlantDetails plantDetails)
        {
            return new PlantDetailsViewModel()
            {
                PlantDetailsId = plantDetails.PlantDetailsId,
                Name = plantDetails.Name,
                Description = plantDetails.Description,
                Category = _context.PlantCategories.Find(plantDetails.CategoryId)?.Category ?? string.Empty,
                Price = plantDetails.Price,
                Weight = plantDetails.Weight,
                Unit = _context.WeightUnits.Find(plantDetails.UnitId)?.Unit ?? string.Empty,
                Stock = plantDetails.Stock,
                ImageName = plantDetails.ImageName,
                ImageUri = _plantImageService.GetPlantImageUri(plantDetails.ImageName),
                ProducerId = plantDetails.ProducerId
            };
        }

        private List<PlantDetailsViewModel> MapToViewModels(List<PlantDetails> plantDetails)
        {
            return plantDetails.Select(d => MapToViewModel(d)).ToList();
        }

        private void GetChangesFromViewModel(PlantDetails plantDetails, CreateUpdatePlantDetailsViewModel viewModel)
        {
            var category = _context.PlantCategories.Where(c => EF.Functions.Like(c.Category, viewModel.Category ?? string.Empty)).FirstOrDefault();
            var weightUnit = _context.WeightUnits.Where(c => EF.Functions.Like(c.Unit, viewModel.Unit ?? string.Empty)).FirstOrDefault();

            plantDetails.Name = viewModel.Name;
            plantDetails.Description = viewModel.Description;
            plantDetails.CategoryId = category?.Id ?? -1;
            plantDetails.Price = viewModel.Price;
            plantDetails.Weight = viewModel.Weight;
            plantDetails.UnitId = weightUnit?.Id ?? -1;
            plantDetails.Stock = viewModel.Stock;
        }
    }
}
