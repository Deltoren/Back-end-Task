using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
	public class Film
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public String Name { get; set; }

		public String Description { get; set; }

		public String ReleaseDate { get; set; }

		public String Producer { get; set; }

		public String CreatorId { get; set; }

		public String PosterPath { get; set; }
	}
}
