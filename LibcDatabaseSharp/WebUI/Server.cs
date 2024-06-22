using System.Reflection;
using LibcDatabaseSharp.Class;
using LibcDatabaseSharp.Generic;
using LibcDatabaseSharp.WebUI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace LibcDatabaseSharp.WebUI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            
            services.AddRouting();
            services.AddControllers()
                .AddApplicationPart(typeof(ProcessController).Assembly)
                .AddApplicationPart(typeof(ApiController).Assembly);
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseCors("AllowAnyOrigin");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.StartsWith("/api"))
                {
                    await next();
                }
                else
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    string resourceName = GetResourceName(context.Request.Path.Value);
                    if (resourceName != null)
                    {
                        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                context.Response.ContentType = GetContentType(resourceName);
                                await stream.CopyToAsync(context.Response.Body);
                                return;
                            }
                        }
                    }

                    context.Response.StatusCode = 404;
                }
            });
        }
        
        private string GetResourceName(string requestPath)
        {
            var resourceName = "LibcDatabaseSharp.WebUI" + requestPath.Replace("/", ".");
            if (requestPath == "/" || requestPath.EndsWith("/Index.html"))
            {
                resourceName = "LibcDatabaseSharp.WebUI.Index.html";
            }
            else if (requestPath.Contains("libc-details"))
            {
                resourceName = "LibcDatabaseSharp.WebUI.libc-details.html";
            }
            return resourceName;
        }

        private string GetContentType(string resourceName)
        {
            if (resourceName.EndsWith(".html"))
                return "text/html";
            if (resourceName.EndsWith(".js"))
                return "application/javascript";
            if (resourceName.EndsWith(".css"))
                return "text/css";
            if (resourceName.EndsWith(".png"))
                return "image/png";
            if (resourceName.EndsWith(".jpg") || resourceName.EndsWith(".jpeg"))
                return "image/jpeg";
            if (resourceName.EndsWith(".gif"))
                return "image/gif";

            return "application/octet-stream";
        }
    }
}

namespace LibcDatabaseSharp.WebUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessController : ControllerBase
    {
        [HttpPost("FindMatchingLibc")]
        public IActionResult FindMatchingLibc([FromBody] List<SearchParam> searchParams)
        {
            try
            {
                var funcNames = searchParams.Select(p => p.FuncName).ToList();
                var offsets = searchParams.Select(p => p.FuncOffset).ToList();

                var matchingLibcs = API.Handler.GetMatchingLibc(funcNames, offsets);

                if (matchingLibcs == null || !matchingLibcs.Any())
                {
                    return NotFound(new { message = "No matching libc found." });
                }
                
                var result = matchingLibcs.Select(libc => new 
                {
                    libc.Name,
                    libc.Arch,
                    libc.Version,
                    libc.IsDebug
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [HttpPost("GetLibcDetails")]
        public IActionResult GetLibcDetails([FromBody] RequestLibc request)
        {
            try
            {
                var libcDetails = API.Handler.GetLibcDetails(request.LibcName);

                if (libcDetails == null)
                {
                    return NotFound(new { message = "Libc details not found." });
                }
        
                return Ok(libcDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        [HttpGet("GetMatchingLibc")]
        public IActionResult GetMatchingLibc([FromQuery] string[] funcNames, [FromQuery] string[] funcOffsets)
        {
            try
            {
                var matchingLibcs = API.Handler.GetMatchingLibc(funcNames.ToList(), funcOffsets.ToList());

                if (matchingLibcs == null || !matchingLibcs.Any())
                {
                    return NotFound(new { message = "No matching libc found." });
                }

                var result = matchingLibcs.Select(libc => libc.Name);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetOffsetByLibc")]
        public IActionResult GetOffsetByLibc([FromQuery] string libcName, [FromQuery] string[] funcNames)
        {
            if (string.IsNullOrEmpty(libcName) || funcNames == null || funcNames.Length == 0)
            {
                return BadRequest(new { message = "Libc name and function names are required." });
            }

            var funcList = new List<SearchParam>();

            foreach (var func in funcNames)
            {
                string address = API.Handler.GetFuncOffset(libcName, func);

                if (address != null)
                {
                    funcList.Add(new SearchParam
                    {
                        FuncName = func,
                        FuncOffset = address
                    });
                }
            }

            if (funcList.Count == 0)
            {
                return NotFound(new { message = "No functions found in the specified libc." });
            }

            return Ok(funcList);
        }
    }
}