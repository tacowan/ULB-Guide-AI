using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Plugins;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Planning;


public class ReservationSystemSkill {

    private PNR pnr;
    private PNR[] testData;

    private ContextVariables context;

    public ContextVariables Context { get => context; set => context = value; }

    public ReservationSystemSkill() { 
        
    }

    public ReservationSystemSkill(PNR[] testData) { 
        this.testData = testData;    
    }

    public ReservationSystemSkill(string testData) { 
        this.testData = JsonSerializer.Deserialize<PNR[]>(testData);    
    }   

    [SKFunction, Description("Change to aisle seat")]
    public async Task<string> changeAisleSeat(string pnr_number)
    {
        if (pnr == null)
            return "{ \"error\" : \"There is no PNR in context. Require find by name.\"}";
        pnr.seat = "21C";
        pnr.action = "Seat changed to aisle seat " + pnr.seat;
        return serializePNR();
    }

    [SKFunction, Description("Given a new seat number, change seat")]
    public async Task<string> changeSeat(
        [Description("new seat number")] string seat)
    {
        if (pnr == null)
            return "{ \"error\" : \"There is no PNR in context. Require find by name.\"}";

        pnr.action = $"Seat changed from {pnr.seat} to {seat}";
        pnr.seat = seat;
        return serializePNR();
    }

    [SKFunction, Description("Given a person's first and last name, search for a PNR record.")]
    public async Task<string> findPNRAsync(
        [Description("The last name of the person whose PNR needs to be found")] string lastname,
        [Description("The first name of the person whose PNR needs to be found")] string firstname
    )
    {
        Console.WriteLine("findPNRAsync called with " + lastname + ", " + firstname);
        // random integer between 0 and 6
        int index = new Random().Next(0, 6);
        this.pnr = testData[index];
        pnr.passenger.firstName = firstname;
        pnr.passenger.lastName = lastname;

        // we're not really finding a record, just returning a random one
        // but the chat and data must be consistent
        pnr.action = "PNR found for " + firstname + " " + lastname;
        
        // convert PNR to JSON string
        return serializePNR();
            
    }

    private string serializePNR() {
        var jsonData =  JsonSerializer.Serialize(pnr);  
        context.Set("pnr", jsonData);   
        return jsonData;   
    }


    /**
    * return a sample PNR to help LLM understand the data structure
    */
    public static string getSampleData() {
        PNR sample = new PNR();
        sample.pnr = "FGDHKL";
        sample.passenger = new Passenger();
        sample.passenger.firstName = "Jill";
        sample.passenger.lastName = "Smith";
        sample.flight = new Flight();
        sample.flight.flightNumber = "UA123";
        sample.flight.departureAirport = "SFO";
        sample.flight.arrivalAirport = "LAX";
        sample.flight.departureDate = DateTime.Now;
        sample.flight.arrivalDate = DateTime.Now.AddHours(1);
        sample.seat = "12A";
        sample.@class = "Economy";
        sample.action = $"PNR found for {sample.passenger.lastName}, {sample.passenger.firstName} ";
        return JsonSerializer.Serialize(sample);
    }

    public static bool hasParameters(Plan plan) {
        Plan step = plan.Steps.First<Plan>();
        foreach(var param in step.Parameters) {
            if (param.Value != null)  {
                var str = param.Value.ToString().ToLower();
                if( str.Contains("name") ) {
                    return false;
                }
            }
        }
        return step.Parameters.Count > 0;
    }

}

public class PNR {
    public string action { get; set; }
    public string pnr { get; set; }
    public Passenger passenger { get; set; }
    public Flight flight { get; set; }
    public string seat { get; set; }
    public string @class { get; set; }
    
}

public class Passenger {
    public string firstName { get; set; }
    public string lastName { get; set; }
}

public class Flight {
    public string flightNumber { get; set; }
    public string departureAirport { get; set; }
    public string arrivalAirport { get; set; }
    public DateTime departureDate { get; set; }
    public DateTime arrivalDate { get; set; }
}