using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LanBackup.ModelsCore;
using LanBackup.DataCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using AutoMapper;
using LanBackup.WebApp.Models.DTO;
using AutoMapper.QueryableExtensions;
using LanBackup.WebApp.Models.Telemetry;

namespace LanBackup.WebApp.Controllers
{

  /// <summary>
  /// BackupConfig REST Web api controller for handling backup configurations
  /// </summary>
  [Authorize]
  [Produces("application/json")]
  [Route("api/[controller]")]
  public class BackupConfigController : Controller
  {

    private readonly IMapper mapper;
    private BackupsContext _context;
    private ITelemetryLogger telemetry;

    public BackupConfigController(IMapper mapper, BackupsContext context, ITelemetryLogger telemetry)
    {
      this.mapper = mapper;
      this._context = context;
      this.telemetry = telemetry;
    }


    /// <summary>
    /// retrieve all  backup configurations
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BackupConfigurationDTO>), 200)]
    [ProducesResponseType(typeof(PaginatedList<BackupConfiguration, String, BackupConfigurationDTO>), 200)]
    public async Task<IActionResult> Get([FromHeader] string idx, [FromHeader] string siz)
    //public IEnumerable<BackupConfiguration> Get()
    {
      //return new DatabaseManager(_context).GetBackupConfigs();
      if (!string.IsNullOrEmpty(idx) && !string.IsNullOrEmpty(siz))
      {
        return new OkObjectResult(
          (await PaginatedList<BackupConfiguration, String, BackupConfigurationDTO>.CreateAsync(_context.Backups, Convert.ToInt32(idx), Convert.ToInt32(siz), o=>o.ClientIP, this.mapper))
          );
      }
      return new OkObjectResult(_context.Backups.ProjectTo<BackupConfigurationDTO>().ToList());
    }

    /// <summary>
    /// retrieve backup configuration by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BackupConfigurationDTO), 200)]
    [ProducesResponseType(typeof(BackupConfigurationDTO), 404)]
    public IActionResult Get(string id)
    {
      var res = new DatabaseManager(_context).GetBackupConfig(id);
      if (res == null)
      {
        return NotFound();
      }
      var mapped = mapper.Map<BackupConfiguration, BackupConfigurationDTO>(res);
      return new ObjectResult(mapped);
    }

    /// <summary>
    /// retrieve backup configuration by ID
    /// </summary>
    /// <param name="clientid"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("client/{clientid}")]
    [ProducesResponseType(typeof(IEnumerable<BackupConfigurationDTO>), 200)]
    [ProducesResponseType(typeof(IEnumerable<BackupConfigurationDTO>), 404)]
    public IActionResult GetByCient(string clientid)
    {
      var results = new DatabaseManager(_context).GetBackupConfigByClient(clientid);
      if (results == null || results.Count() == 0)
        return NotFound();
      var mapped = mapper.Map<IEnumerable<BackupConfiguration>, IEnumerable<BackupConfigurationDTO>>(results);
      return new ObjectResult(mapped);
    }


    /// <summary>
    /// create backup configuration
    /// </summary>
    /// <param name="backupDTO"></param>
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(BackupConfigurationDTO), 200)]
    [ProducesResponseType(typeof(BackupConfigurationDTO), 400)]
    public async Task<IActionResult> Create([FromBody]BackupConfigurationDTO backupDTO)
    {
      if (ModelState.IsValid)
      {
        try
        {
          var backup = mapper.Map<BackupConfigurationDTO, BackupConfiguration>(backupDTO);
          _context.Add(backup);
          var res = await _context.SaveChangesAsync();
          if (res > 0)
          {
            var mapped = mapper.Map<BackupConfiguration,BackupConfigurationDTO>(backup);
            return new ObjectResult(mapped);
          }
        }
        catch (Exception ex)
        {
          this.telemetry.TrackException(ex);
          return BadRequest(
            new { message = ex.Message }
          );
        }
      }
      else
      {
        string msg = string.Join("|", ModelState.Values.SelectMany(e => e.Errors).Select(s => s.ErrorMessage));
        return BadRequest(msg);
      }
      return BadRequest();
    }


    /// <summary>
    /// Update backup configuration
    /// </summary>
    /// <param name="id"></param>
    /// <param name="backup"></param>
    /// <returns>affected records</returns>
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BackupConfigurationDTO), 200)]
    [ProducesResponseType(typeof(BackupConfigurationDTO), 400)]
    public async Task<IActionResult> Update(string id, [FromBody]BackupConfigurationDTO backupDTO)
    {
      if (id != backupDTO.ID)
      {
        return BadRequest("Ids do not match");
      }
      if (ModelState.IsValid)
      {
        var backup = mapper.Map<BackupConfigurationDTO, BackupConfiguration>(backupDTO);
        _context.Update(backup);
        try
        {
          var res = await _context.SaveChangesAsync();
          if (res > 0)
          {
            var mapped = mapper.Map<BackupConfiguration, BackupConfigurationDTO>(backup);
            return new ObjectResult(mapped);//retun back the updated object
          }
        }
        catch (Exception ex)
        {
          this.telemetry.TrackException(ex);
          return BadRequest(
            new { message = ex.Message }
          );

        }
      }
      else
      {
        string msg = string.Join("|", ModelState.Values.SelectMany(e => e.Errors).Select(s => s.ErrorMessage));
        return BadRequest(msg);
      }
      return BadRequest();
    }

    /// <summary>
    /// delete backup configuration by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 404)]
    public async Task<IActionResult> Delete(string id)
    {
      try
      {
        BackupConfiguration backup = _context.Backups.Find(id);
        _context.Backups.Remove(backup);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
        {
          var mapped = mapper.Map<BackupConfiguration, BackupConfigurationDTO>(backup);
          return new ObjectResult(mapped);//retun back old Object
        }
        return NotFound(id);
      }
      catch (Exception ex)
      {
        this.telemetry.TrackException(ex);
        return BadRequest(
            new { message = ex.Message }
          );
      }
      return NotFound(id);
    }
  }
}
