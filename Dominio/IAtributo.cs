﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
	public interface IAtributo
	{
		int Id { get; set; }
		string Descripcion { get; set; }
		string ToString();
	}
}
