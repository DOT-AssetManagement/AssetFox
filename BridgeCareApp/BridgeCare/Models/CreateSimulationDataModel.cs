using System.ComponentModel.DataAnnotations;

namespace BridgeCare.Models
{
    public class CreateSimulationDataModel
    {
        [Required]
        public int NetworkId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Owner { get; set; }

        [Required]
        public string Creator { get; set; }
    }
}
