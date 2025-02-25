using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WebApplication.Data;      
using WebApplication.Models;  
using LinqToDB;                

namespace WebApplication.Controllers
{
    
    [ApiController]
    [Route("customers")]
    public class CustomersController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public CustomersController(DatabaseContext db)
        {
            _db = db;
        }

        
        public class CustomerCreateDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
       
        [HttpPost]
        public IActionResult CreateCustomer([FromBody] CustomerCreateDto request)
        {
           
            var entity = new Customer
            {
                Name = request.Name,
                Email = request.Email
            };

            var newId = _db.InsertWithIdentity(entity);
            entity.Id = Convert.ToInt32(newId);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = entity.Id },
                entity
            );
        }

       
       
        [HttpGet("{id}")]
        public IActionResult GetCustomerById(int id)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
                return NotFound(); 

            return Ok(customer); 
        }
    }
}
