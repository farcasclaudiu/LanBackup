using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LanBackup.ModelsCore;
using LanBackup.DataCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using LanBackup.WebApp.Models.DTO;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LanBackup.WebApp.Models.Telemetry;

namespace LanBackup.WebApp.Controllers
{

  /// <summary>
  /// BackupLogs REST Web api controller for handling logs
  /// </summary>
  [Authorize]
  [Produces("application/json")]
  [Route("api/[controller]")]
  public class LogsController : Controller
  {
    private readonly IMapper mapper;
    private BackupsContext _context;
    private ITelemetryLogger telemetry;


    public LogsController(IMapper mapper, BackupsContext context, ITelemetryLogger telemetry)
    {
      this.mapper = mapper;
      this._context = context;
      this.telemetry = telemetry;
    }


    ///// <summary>
    ///// retrieves all logs from DB
    ///// </summary>
    ///// <returns></returns>
    //[AllowAnonymous]
    //[HttpGet]
    //[ProducesResponseType(typeof(IEnumerable<BackupLog>), 200)]
    //public IActionResult Get()
    //{
    //  return new OkObjectResult(_context.Logs.ToList());
    //  //return new  ObjectResult(_context.Logs.ToList());
    //}


    /// <summary>
    /// retrieves paginated logs from DB
    /// </summary>
    /// <param name="idx">page index</param>
    /// <param name="siz">page size</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet]
    //[HttpGet("{pageIndex}/{pageSize}")]
    [ProducesResponseType(typeof(IEnumerable<BackupLogDTO>), 200)]
    [ProducesResponseType(typeof(PaginatedList<BackupLog, DateTime, BackupLogDTO>), 200)]
    public async Task<IActionResult> Get([FromHeader] string idx, [FromHeader] string siz)
    {
      //StringValues val1, val2;
      //HttpContext.Request.Headers.TryGetValue("idx", out val1) &&
      //HttpContext.Request.Headers.TryGetValue("siz", out val2)
      if (!string.IsNullOrEmpty(idx) && !string.IsNullOrEmpty(siz))
      {
        return new OkObjectResult(
          (await PaginatedList<BackupLog, DateTime, BackupLogDTO>.CreateAsync(_context.Logs, Convert.ToInt32(idx), Convert.ToInt32(siz), (or)=> or.DateTime, this.mapper))
          );
      }
      return new OkObjectResult(_context.Logs.ProjectTo<BackupLogDTO>().ToList());
    }


    /// <summary>
    /// retrieves alog record by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BackupLogDTO), 200)]
    [ProducesResponseType(typeof(BackupLogDTO), 404)]
    public IActionResult Get(int id)
    {
      var result = _context.Logs.SingleOrDefault(p => p.ID == id);
      if (result == null)
      {
        return NotFound();
      }
      var mapped = mapper.Map<BackupLog, BackupLogDTO>(result);
      return new OkObjectResult(mapped);
    }

    
    /// <summary>
    /// retrieve all logs of a clientIP
    /// </summary>
    /// <param name="clientid"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("client/{clientid}")]
    [ProducesResponseType(typeof(IEnumerable<BackupLogDTO>), 200)]
    [ProducesResponseType(typeof(IEnumerable<BackupLogDTO>), 404)]
    public IActionResult GetByCientID(string clientid)
    {
      var results = _context.Logs.Where(p => p.ClientIP == clientid).OrderByDescending(o => o.DateTime);
      if (results == null || results.Count() == 0)
        return NotFound();
      var mapped = mapper.Map<IEnumerable<BackupLog>, IEnumerable<BackupLogDTO>>(results);
      return new OkObjectResult(mapped);
    }


    /// <summary>
    /// retrieves all logs of a specific configuration
    /// </summary>
    /// <param name="configurationid"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("config/{configurationid}")]
    [ProducesResponseType(typeof(IEnumerable<BackupLogDTO>), 200)]
    [ProducesResponseType(typeof(IEnumerable<BackupLogDTO>), 404)]
    public IActionResult GetByConfigurationID(string configurationid)
    {
      var results = _context.Logs.Where(p => p.ConfigurationID == configurationid).OrderByDescending(o => o.DateTime);
      if (results == null || results.Count() == 0)
        return NotFound();
      var mapped = mapper.Map<IEnumerable<BackupLog>, IEnumerable<BackupLogDTO>>(results);
      return new ObjectResult(mapped);
    }



    //[EnableCors("AllowSpecificOrigin")]
    /// <summary>
    /// Create an entry in the logs
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    [AllowAnonymous]//[Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(typeof(int), 400)]
    public async Task<IActionResult> Create([FromBody]BackupLog log)
    {
      if (ModelState.IsValid)
      {
        _context.Logs.Add(log);
        var res = await _context.SaveChangesAsync();
        if (res >0 )
          return new ObjectResult(log.ID);//retun back the new ID
      }
      else
      {
        string msg = string.Join("|", ModelState.Values.SelectMany(e => e.Errors).Select(s => s.ErrorMessage));
        return BadRequest(msg);
      }
      return BadRequest();
    }



  }
}
