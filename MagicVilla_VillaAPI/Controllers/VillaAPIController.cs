using AutoMapper;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/villaAPI")]
    [ApiController]
    public class VillaAPIController:ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAPIController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
            return Ok(_mapper.Map<List<VillaDTO>>(villaList));
             
        }


        [HttpGet("{id:int}",Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDTO>> GetVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<VillaDTO>(villa));
        }



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody]VillaCreateDTO createDTO)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa Already Exists");
                return BadRequest(ModelState);
            }
            if (createDTO == null)
            {
                return BadRequest(createDTO);
            }
            Villa model=_mapper.Map<Villa>(createDTO);
            //Villa model = new()
            //{
            //    Amenity = createDTO.Amenity,
            //    Details = createDTO.Details,
            //    Name = createDTO.Name,
            //    ImageUrl = createDTO.ImageUrl,
            //    Occupancy = createDTO.Occupancy,
            //    Rate = createDTO.Rate,
            //    Sqft = createDTO.Sqft,
            //};
           await _db.Villas.AddAsync(model);
           await _db.SaveChangesAsync();

            return CreatedAtRoute("GetVilla", new {id=model.Id},model);
        }



        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id:int}",Name ="DeleteVilla")]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            if(id==0)
            {
                return BadRequest();
            }
            var villa=await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            _db.Villas.Remove(villa);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id:int}", Name = "UpdateVilla")]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody]VillaUpdateDTO updateDTO)
        {
            if(updateDTO == null || id != updateDTO.Id)
            {
                return BadRequest();
            }

            Villa model=_mapper.Map<Villa>(updateDTO);
  
            _db.Villas.Update(model);
           await _db.SaveChangesAsync();
            return NoContent();

        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if(patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
           
            if (villa == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(villaDTO,ModelState);
            Villa model = _mapper.Map<Villa>(villaDTO);

            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }
    }
}
