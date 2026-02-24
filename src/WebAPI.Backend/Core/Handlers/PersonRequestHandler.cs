using System.Diagnostics;

namespace WebAPI.Backend.Core.Handlers;

//public class PersonRequestHandler(IPeopleService peopleService, ILogger<PersonRequestHandler> logger)
//{
//    public async Task<PersonResponse> HandleAsync(PersonRequest request)
//    {
//        try
//        {
//            var person = await peopleService.GetItemAsync(request.Id);
//            return new PersonResponse { Person = person };
//        }
//        catch (Exception ex)
//        {
//            // log e rilancia (così entra nella retry/DLQ)
//            logger.LogError(ex, "Error handling PersonRequest Id={Id}", request.Id);
//            throw;
//        }
//    }
//}

public class PersonRequestHandler(IPeopleService peopleService, ILogger<PersonRequestHandler> logger)
{
    public async Task<PersonResponse> HandleAsync(PersonRequest request)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Handling PersonRequest for Id={Id}", request?.Id);
            var person = await peopleService.GetItemAsync(request.Id);
            var response = new PersonResponse { Person = person };
            sw.Stop();
            logger.LogInformation("Handled PersonRequest Id={Id} successfully in {ElapsedMs}ms, found={HasPerson}", request.Id, sw.ElapsedMilliseconds, response.Person != null);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error handling PersonRequest Id={Id} after {ElapsedMs}ms", request?.Id, sw.ElapsedMilliseconds);
            throw;
        }
    }
}