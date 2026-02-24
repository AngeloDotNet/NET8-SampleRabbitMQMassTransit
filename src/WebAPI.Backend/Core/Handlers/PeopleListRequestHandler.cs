using System.Diagnostics;

namespace WebAPI.Backend.Core.Handlers;

//public class PeopleListRequestHandler(IPeopleService peopleService)
//{
//    public async Task<PeopleListResponse> HandleAsync(PeopleListRequest request)
//    {
//        try
//        {
//            var list = await peopleService.GetListItemAsync();
//            return new PeopleListResponse { People = list };
//        }
//        catch (Exception ex)
//        {
//            // Log the exception (logging mechanism not shown here)
//            throw new ApplicationException("An error occurred while processing the people list request.", ex);
//        }
//    }
//}

public class PeopleListRequestHandler(IPeopleService peopleService, ILogger<PeopleListRequestHandler> logger)
{
    public async Task<PeopleListResponse> HandleAsync(PeopleListRequest request)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Handling PeopleListRequest");
            var list = await peopleService.GetListItemAsync();
            var response = new PeopleListResponse { People = list };
            sw.Stop();
            logger.LogInformation("Handled PeopleListRequest successfully in {ElapsedMs}ms, items={Count}", sw.ElapsedMilliseconds, response?.People?.Count ?? 0);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error handling PeopleListRequest after {ElapsedMs}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }
}