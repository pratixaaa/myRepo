using Microsoft.AspNetCore.Mvc;

namespace ShipandPort.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipsController : ControllerBase
    {
        //storing dynamic data 
        private static List<Ship> ships = new List<Ship>();

        //Adding static data for port details
        private static List<Port> ports = new List<Port>
        {
            new Port
            {
                PortId = 1,
                PortName = "Kandla Port",
                PortLatitude = 23.00,
                PortLongitude = 70.18
            },
            new Port
            {
                PortId = 2,
                PortName = "Mundra Port",
                PortLatitude = 22.74,
                PortLongitude = 69.70
            },
            new Port
            {
                PortId = 3,
                PortName = "Jamanagar Port",
                PortLatitude = 22.47,
                PortLongitude = 70.05
            }
        };

        //Create new data for ship
        [HttpPost]
        public IActionResult AddShip([FromBody] Ship ship)
        {
            if (ships.Any(s => s.Id == ship.Id))
            {
                return Conflict("A ship with the same ID already exists.");
            }

            if (string.IsNullOrWhiteSpace(ship.Name))
            {
                return BadRequest(new { message = "Please enter a non-empty name." });
            }

            if (ship.Velocity == 0)
            {
                return BadRequest(new { message = "Please enter a non-zero velocity." });
            }

            if (ship.Latitude == 0)
            {
                return BadRequest(new { message = "Please enter a non-zero value for Latitude." });
            }

            if (ship.Longitude == 0)
            {
                return BadRequest(new { message = "Please enter a non-zero value for Longitude." });
            }

            ship.Id = ships.Count + 1;

            ships.Add(ship);

            return Ok(new { message = "Ship added successfully.", ship = ship });
        }

        //Update ship data 
        [HttpPut("{id}")]
        public IActionResult UpdateShip(int id, [FromBody] Ship ship)
        {
            var existingShip = ships.FirstOrDefault(s => s.Id == id);

            if (existingShip == null)
            {
                return NotFound();
            }

            if (ship.Id != id && ships.Any(s => s.Id == ship.Id))
            {
                return Conflict("A ship with the same ID already exists.");
            }

            if (string.IsNullOrWhiteSpace(ship.Name))
            {
                return BadRequest(new { message = "Please enter a non-empty name." });
            }

            if (ship.Velocity == 0)
            {
                return BadRequest(new { message = "Please enter a non-zero velocity." });
            }

            if (ship.Latitude == 0)
            {
                return BadRequest(new { message = "Please enter a non-zero value for Latitude." });
            }

            if (ship.Longitude == 0)
            {
                return BadRequest(new { message = "Please enter a non-zero value for Longitude." });
            }

            existingShip.Name = ship.Name;
            existingShip.Velocity = ship.Velocity;
            existingShip.Latitude = ship.Latitude;
            existingShip.Longitude = ship.Longitude;

            return Ok(new { message = "Ship updated successfully.", ship = existingShip });
        }

        //Get all Ship details
        [HttpGet]
        public IActionResult GetAllShips()
        {
            string message = "";
            if (ships.Count == 0)
            {
                message = "No ship available";
            }
            else
            {
                message = "Ship details";
            }
            return Ok(new { message, ships });
        }

        //Update or change ship velocity 
        [HttpPut("{id}/velocity")]
        public IActionResult UpdateShipVelocity(int id, [FromBody] double velocity)
        {
            if (velocity == 0)
            {
                return BadRequest(new { message = "Please enter a non-zero velocity." });
            }
            Ship shipToUpdate = null;

            foreach (var ship in ships)
            {
                if (ship.Id == id)
                {
                    shipToUpdate = ship;
                    break;
                }
            }
           
            if (shipToUpdate == null)
            {
                return NotFound();
            }
            shipToUpdate.Velocity = velocity;

            return Ok(new { message = "Ship velocity updated successfully.", ship = shipToUpdate });
        }

        //Remove ship data
        [HttpDelete("{id}")]
        public IActionResult RemoveShip(int id)
        {
            var shipToRemove = ships.SingleOrDefault(ship => ship.Id == id);
            if (shipToRemove == null)
            {
                return Conflict("Ship not exists.");
            }

            ships.Remove(shipToRemove);

            return Ok(new { message = "Ship removed successfully.", ship = shipToRemove });
        }

        //Find nearest port of ship
        [HttpGet]
        [Route("api/ships/{shipId}/closestPort")]
        public IActionResult GetClosestPort(int shipId)
        {
            // Find the ship with the specified ID
            //Ship ship = null;

            var ship = ships.FirstOrDefault(s => s.Id == shipId);
            if (ship == null)
            {
                return NotFound("Ship not found.");
            }


            foreach (var s in ships)
            {
                if (s.Id == shipId)
                {
                    ship = s;
                    break;
                }
            }

            // Calculate the distance between the ship and each port
            List<dynamic> distances = new List<dynamic>();
            foreach (var p in ports)
            {
                var distance = new
                {
                    Port = p,
                    Distance = CalculateDistance(ship.Latitude, ship.Longitude, p.PortLatitude, p.PortLongitude)
                };
                distances.Add(distance);
            }

            // Find the closest port
            dynamic closestPort = null;


            double shortestDistance = double.MaxValue;
            foreach (var d in distances)
            {
                if (d.Distance < shortestDistance)
                {
                    shortestDistance = d.Distance;
                    closestPort = d.Port;
                }
            }

            // Calculate the estimated arrival time based on the velocity of the ship and the distance to the closest port
            var estimatedArrivalTime = CalculateEstimatedArrivalTime(ship.Velocity, shortestDistance);

            // Return the details of the closest port and estimated arrival time
            return Ok(new
            {
                PortName = closestPort.PortName,
                PortLatitude = closestPort.PortLatitude,
                PortLongitude = closestPort.PortLongitude,
                EstimatedArrivalTime = estimatedArrivalTime,
                message = "Your closest port is " + closestPort.PortName + ""
            });
        }

        //Calculate Estimated Arrival Time 
        private TimeSpan CalculateEstimatedArrivalTime(double velocity, double distance)
        {
            var estimatedTravelTime = distance / velocity;
            return TimeSpan.FromHours(estimatedTravelTime);
        }

        // Helper method to calculate the distance between two sets of latitude and longitude coordinates
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        // Helper method to convert degrees to radians
        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

    }
}
