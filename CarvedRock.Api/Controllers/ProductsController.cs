using System.Collections.Generic;
using CarvedRock.Api.ApiModels;
using CarvedRock.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CarvedRock.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductLogic _productLogic;
        //private readonly ILogger _logger;

        public ProductsController(/*ILogger<ProductsController> logger,*/ IProductLogic productLogic)
        {
            _productLogic = productLogic;
            //_logger = logger;
        }

        [HttpGet]
        public IEnumerable<Product> GetProducts(string category = "all")
        {
            // _logger.LogInformation("Starting controller action GetProducts for {category}", category);
            //Log.Information("Starting controller action GetProducts for {category}", category);
            // Log.ForContext("Category", category)
            //    .Information("Starting controller action GetProducts");
            return _productLogic.GetProductsForCategory(category);
        }
    }
}