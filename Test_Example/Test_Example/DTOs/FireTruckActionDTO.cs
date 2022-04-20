using Test_Example.Models
namespace Test_Example.DTOs
{
    public class FireTruckActionDTO
    {
        public FireTruck FireTruck { get; set; }
        public List<ActionFiremenDTO> Actions { get; set; }
    }
}