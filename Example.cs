using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
//using Dapper;
using System.Data;

public class Example 
{ 
    //public static async Task Main(string[] args) 
    //{ 
    //    var host = CreateHostBuilder(args).Build(); 
    //    var config = host.Services.GetRequiredService<IConfiguration>(); 
    //    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"))) 
    //    { 
    //        await connection.OpenAsync(); var transaction = connection.BeginTransaction(); 
    //        try 
    //        { 
    //            var api = host.Services.GetRequiredService<Api>(); 
    //            var colInfoSql = await File.ReadAllTextAsync("./sql/SELECT_EDocLibraryTemplate_INFO_COLS.sql"); 
    //            var colInfos = await connection.QueryAsync(colInfoSql, transaction: transaction); 
    //            var colNames = colInfos.Select(ci => ci.COLUMN_NAME).ToList(); 
    //            var table = new Table("#__TemplateImport", colInfos); 
    //            var templateses = await GetLibraryTemplates(api); 
    //            foreach (var instance in templateses) 
    //            { 
    //                foreach (var template in instance.Templates) 
    //                { 
    //                    template.InstanceId = instance.InstanceId; 
    //                    template.IsActive *= 1; 
    //                    template.FullScreenWindow = Enum.Parse<ERestriction>(template.FullScreenWindow, true); 
    //                    table.AddRow(template); 
    //                } 
    //            } 
    //            await connection.ExecuteAsync("CREATE_Import", table.GetDataTable(), transaction: transaction, commandType: System.Data.CommandType.StoredProcedure); 
    //            transaction.Commit(); 
    //        } 
    //        catch (Exception ex) 
    //        { 
    //            transaction.Rollback(); 
    //            Console.WriteLine($"An error occurred: {ex.Message}"); 
    //        } 
    //    }
    //    await host.RunAsync(); 
    //} 
    //private static async Task<List<Templates>> GetLibraryTemplates(Api api) 
    //{ 
    //    var instances = api.Config.Api.Instances; var tasks = new List<Task<Templates>>(); 
    //    foreach (var instance in instances) 
    //    {
    //        var request = new AuthSettings { Email = instance.Auth.Email, Password = instance.Auth.Password }; 
    //        tasks.Add(api.GetLibraryTemplates(request)); 
    //    } 
    //    return (await Task.WhenAll(tasks)).ToList(); 
    //} 
    //private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureAppConfiguration((hostContext, config) => { config.SetBasePath(Directory.GetCurrentDirectory()); config.AddJsonFile("appsettings.json", optional: true); config.AddEnvironmentVariables(prefix: "PREFIX_"); config.AddCommandLine(args); }).ConfigureServices((hostContext, services) => { services.AddSingleton(hostContext.Configuration); services.AddSingleton<Api>(); }).ConfigureLogging((hostContext, logging) => { logging.AddConfiguration(hostContext.Configuration.GetSection("Logging")); logging.AddConsole(); logging.AddDebug(); }); 
}
//public enum ERestriction { Prohibited, Allowed, Required }
//public class Templates { public int InstanceId { get; set; } public List<Template> Templates { get; set; } }
//public class Template { public int DocId { get; set; } public string DocPassword { get; set; } public string DocName { get; set; } public string DocDescription { get; set; } public string DocCategory { get; set; } public bool IsActive { get; set; } public int DocPageCount { get; set; } public int DocWidth { get; set; } public int DocHeight { get; set; } public int DocBleed { get; set; } public string ClientType { get; set; } public int UserId { get; set; } public ERestriction FullScreenWindow { get; set; } public int InstanceId { get; set; } }
//public class Table { public string Name { get; } public List<dynamic> Rows {     get; } public Table(string name, dynamic rows) { Name = name; Rows = rows; } public DataTable GetDataTable() { var dataTable = new DataTable(Name); var properties = Rows.First().GetType().GetProperties(); foreach (var property in properties) { dataTable.Columns.Add(property.Name, property.PropertyType); } foreach (var row in Rows) { var dataRow = dataTable.NewRow(); foreach (var property in properties) { dataRow[property.Name] = property.GetValue(row); } dataTable.Rows.Add(dataRow); } return dataTable; } }