using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models.ViewModels
{
	public class FilmEditModel
	{
		[Required]
		public String Name { get; set; }

		public String Description { get; set; }

		public String ReleaseDate { get; set; }

		public String Producer { get; set; }

		public IFormFile File { get; set; }
	}
}
