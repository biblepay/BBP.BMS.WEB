using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiblePay.BMS.Models;
using BMSCommon.Model;
using BMSCommon;
using System.IO;

namespace BiblePay.BMS.EndPoints
{
	[ApiController]
	public class MySpecialEndpoint : ControllerBase
    {

		/*
        [HttpPost]
        [Route("BMS/GetPhoneRegions")]
        public async Task<ActionResult<List<DropDownItem>>> GetPhoneRegions()
        {
            string sState = Request.Headers["state"].ToString() ?? String.Empty;
            if (String.IsNullOrEmpty(sState))
            {
                throw new Exception("Unknown state");
            }
            List<DropDownItem> ddRegions = await BBPAPI.DB.PhoneProcs.GetRegionsInternal(sState);
            string sData = Newtonsoft.Json.JsonConvert.SerializeObject(ddRegions);
            return ddRegions;
        }
        

		[HttpPost]
		[Route("BMS/AddNewPhoneUserRemote")]
		public async Task<ActionResult<long>> AddNewPhoneUserRemote()
		{
			string sBBPAddress = Request.Headers["BBPADDRESS"].ToString() ?? String.Empty;
			string sBBPPK = Request.Headers["BBPPK"].ToString() ?? String.Empty;

			if (String.IsNullOrEmpty(sBBPAddress) || String.IsNullOrEmpty(sBBPPK))
			{
				throw new Exception("Unknown state");
			}
            BBPAddressKey bpk = new BBPAddressKey();
            bpk.Address = sBBPAddress;
            bpk.PrivateKey = sBBPPK;
            long nResult = await BBPAPI.Interface.Phone.AddNewPhoneUser(bpk);
            return nResult;
		}
        


		[HttpPost]
		[Route("BMS/BuyAndGetNewPhoneNumber")]
		public async Task<ActionResult<string>> BuyAndGetNewPhoneNumber()
		{
            NewPhoneObject npo = new NewPhoneObject();
            npo.RegionID = (long)(Request.Headers["RegionID"].ToString() ?? String.Empty).ToDouble();
            npo.CountryState = Request.Headers["CountryState"].ToString() ?? String.Empty;
            npo.BBPAddress = Request.Headers["BBPAddress"].ToString() ?? String.Empty;

			if (String.IsNullOrEmpty(npo.CountryState) || String.IsNullOrEmpty(npo.BBPAddress))
			{
				throw new Exception("Unknown CountryState or BBPAddress");
			}
            Common.Log("80001");
            string sResult = await BBPAPI.DB.PhoneProcs.BuyAndGetNewPhoneNumberInternal(npo.RegionID, npo.CountryState, npo.BBPAddress);
            Common.Log("80002 " + sResult);
            return sResult;
		}
        */


	}


	[ApiController]
    [Route("api/roles")]
    public class RolesEndpoint : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _manager;
        private readonly SmartSettings _settings;

        public RolesEndpoint(RoleManager<IdentityRole> manager, SmartSettings settings)
        {
            _manager = manager;
            _settings = settings;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IdentityRole>>> Get()
        {
            var roles = await _manager.Roles.AsNoTracking().ToListAsync();

            return Ok(new { data = roles, recordsTotal = roles.Count, recordsFiltered = roles.Count });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IdentityRole>> Get([FromRoute]string id) => Ok(await _manager.FindByIdAsync(id));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromForm]IdentityRole model)
        {
            model.Id = Guid.NewGuid().ToString();
            model.ConcurrencyStamp = Guid.NewGuid().ToString();

            var result = await _manager.CreateAsync(model);

            if (result.Succeeded)
            {
                return CreatedAtAction("Get", new { id = model.Id }, model);
            }

            return BadRequest(result);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update([FromForm]IdentityRole model)
        {
            var result = await _manager.UpdateAsync(model);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromForm]IdentityRole model)
        {
            // HACK: The code below is just for demonstration purposes!
            // Please use a different method of preventing the default role from being removed
            if (model.Name == _settings.Theme.Role)
            {
                return BadRequest(SmartError.Failed("Please do not delete the default role! =)"));
            }

            var result = await _manager.DeleteAsync(model);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result);
        }
    }
}
