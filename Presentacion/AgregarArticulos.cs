﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dominio;
using Negocio;

namespace Presentacion
{
	public partial class AgregarArticulos : Form
	{
		private Articulo articulo = null;
        private List<Articulo> listaArticulos = null;

        // Control de Imagenes
        private OpenFileDialog file = null;
		private int imagenActual;
		private int imagenActualCarrusel = 0;
		private string rutaImagen = Path.GetDirectoryName(Directory.GetCurrentDirectory().Replace(@"\bin", ""));

		// Banderas de vistas
		private bool esModificar;
		private bool esAgregar;

		// Bandera de cambios en la ventana para cargar lista en el form principal
		public bool hayCambios;

		// Constructor para los casos de vista y modificación de artículo
		public AgregarArticulos(Articulo articulo, bool esModificar = false)
		{
			InitializeComponent();

			if (articulo != null)
			{
				this.articulo = articulo;
			}

			this.esModificar = esModificar;

			// Setea la ventana en modo modificar
			ModoModificar(this.esModificar);
		}

		// Constructor para el caso de agregar artículo
        public AgregarArticulos(bool esAgregar = true)
		{
			InitializeComponent();

			this.esAgregar = esAgregar;

			ModoAgregar(this.esAgregar);
		}

		// Evento de carga inicial de la ventana
		private void Articulos_Load(object sender, EventArgs e)
		{
			timer1.Enabled = true;

			// Lista los articulos para ejecutar valicaciones
			Listararticulos();

			// Esconde las Label de notificaciones en caso de error en la carga de articulos
			InvisibilizarAvisos();
       
            // Carga los combo box de categorías y marcas
            CargarComboBox();

			// Carga los datos del artículo en la ventana si es detalle o modificacion
			if (!this.esAgregar)
				CargarArticulo();
		}

		private void ModoModificar(bool esModificar)
		{
			// Habilitar o deshabilitar cada uno de los campos de texto
			txtCodigo.Enabled = esModificar;
			txtArticulo.Enabled = esModificar;
			txtDescripcion.Enabled = esModificar;
			txtPrecio.Enabled = esModificar;
			cbCategoria.Enabled = esModificar;
			cbMarca.Enabled = esModificar;
			btnCancelar.Visible = esModificar;

			// Control de guardado de imagen
			ControlGuardarImagen();

			// Cargar estilos de la ventana
			CargarEstilos();

			// Seteamos controles de las imagenes
			ImagenControl();

			//Cambiar texto al botón
			if (esModificar)
				btnModificar.Text = "Guardar";
			else
				btnModificar.Text = "Modificar";
		}

		private void ModoAgregar(bool esAgregar)
		{
			if (esAgregar)
			{
               
                btnCancelar.Visible = true;
				btnModificar.Text = "Guardar";
			}
			if (Completotodosloscampos())
			{
                btnModificar.Enabled = true;
            }
			//Control de guardado de imagen
			ControlGuardarImagen();

			//Cargar estilos de la ventana
			CargarEstilos();

			//Seteamos controles de las imagenes
			ImagenControl();
		}

		private void CargarArticulo()
		{
			//Cargar los datos del arítulo en cada uno de sus campos de texto
			txtCodigo.Text = articulo.Codigo;
			txtArticulo.Text = articulo.Nombre;
			txtDescripcion.Text = articulo.Descripcion;
			txtPrecio.Text = articulo.Precio.ToString();
			cbCategoria.SelectedValue = articulo.Categoria.Id;
			cbMarca.SelectedValue = articulo.Marca.Id;

			//Si contiene imagenes, cargarlas
			CargarImagenes();
		}

		//Metodo para cargar las imagenes
		private void CargarImagenes()
		{
			if(articulo.Imagenes?.Count > 0)
			{
				//Guardamos id de la imagen actual por si necesita ser eliminada
				imagenActual = articulo.Imagenes[0].Id;

				//Guardamos el indice de la imagen para poder utilizarla en el carrusel
				imagenActualCarrusel = 0;
				
				CargarPictureBox(pbImagenArticulo, articulo.Imagenes[0].UrlImagen);
			}
			else
			{
				//Si no hay imagenes carga una imagen placeholder
				CargarPictureBox(pbImagenArticulo, rutaImagen + "/img/placeholder.png");
			}
		}

		//Método para cargar el pictureBox
		private void CargarPictureBox(PictureBox pb, string url)
		{
			try
			{
				pb.Load(url);
				btnGuardarImagen.Enabled = true;
				
			}
			catch (Exception ex)
			{
				//Si hay un error carga una imagen placeholder
				pb.Load(rutaImagen + "/img/placeholder.png");
				btnGuardarImagen.Enabled = false;
			}
		}

		private void CargarEstilos()
		{
			//Icono de la ventana
			this.Icon = new System.Drawing.Icon(rutaImagen + "/img/icono.ico");
			
			//Cargar imágenes
			CargarPictureBox(pbFlechaIzquierda, rutaImagen + "/img/flecha_izquierda.png");
			CargarPictureBox(pbFlechaDerecha, rutaImagen + "/img/flecha_derecha.png");

			//Si no hay producto, no se carga el grupo imágenes
			if (articulo == null)
			{
				gbImagen.Visible = false;
			}
			else
			{
				gbImagen.Visible = true;
			}
		}

		private void ImagenControl()
		{

			if(articulo == null)
			{
				pbFlechaIzquierda.Visible = false;
				pbFlechaDerecha.Visible = false;
				btnImagenBorrarActual.Visible = false;

				return;
			}

			if (articulo.Imagenes != null)
			{
				//Si hay imagen
				if (articulo.Imagenes.Count > 0)
				{
					//... habilitar boton de borrar si hay al menos una imagen
					btnImagenBorrarActual.Visible = true;

					//Si hay mas de una imagen, habilita los botones de carrusel
					if (articulo.Imagenes.Count > 1)
					{
						pbFlechaIzquierda.Visible = true;
						pbFlechaDerecha.Visible = true;
					}
					else
					{
						pbFlechaIzquierda.Visible = false;
						pbFlechaDerecha.Visible = false;
					}

					// No mostrar primer boton del carrusel
					if (imagenActualCarrusel == 0)
					{
						pbFlechaIzquierda.Visible = false;
					}
				}
				else
				{
					pbFlechaIzquierda.Visible = false;
					pbFlechaDerecha.Visible = false;
					btnImagenBorrarActual.Visible = false;
				}
			}
		}

		private void CargarComboBox()
		{
			//Formatear los datos de los combo box
			cbMarca.ValueMember = "Id";
			cbMarca.DisplayMember = "Descripcion";
			cbCategoria.ValueMember = "Id";
			cbCategoria.DisplayMember = "Descripcion";
			
			//Cargar combo box de categoria y marca
			comboBoxMarca(cbMarca);
			comboBoxCategoria(cbCategoria);
		}

		//Cargar en el combo box las marcas desde la base de datos
		public static void comboBoxMarca(ComboBox combo)
		{
			try
			{
				IAtributosNegocio marcaNegocio = new MarcaNegocio();
				List<IAtributo> listaMarca = marcaNegocio.listar();

				//Crear opcion vacía
				IAtributo marcaVacia = new Marca();
				marcaVacia.Id = -1;
				marcaVacia.Descripcion = "Seleccione una marca";
				listaMarca.Insert(0, marcaVacia);

				//Cargar ComboBox
				combo.DataSource = listaMarca;
				combo.SelectedIndex = 0;
			}
			catch (Exception)
			{
				MessageBox.Show("Error al cargar las marcas");
			}
		}

		//Cargar en el combo box las categorias desde la base de datos
		public static void comboBoxCategoria(ComboBox combo)
		{
			try
			{
				IAtributosNegocio categoriaNegocio = new CategoriaNegocio();
				List<IAtributo> listaCategoria = categoriaNegocio.listar();

				//Crear opcion vacía
				IAtributo marcaVacia = new Categoria();
				marcaVacia.Id = -1;
				marcaVacia.Descripcion = "Seleccione una categoria";
				listaCategoria.Insert(0, marcaVacia);

				//Cargar ComboBox
				combo.DataSource = listaCategoria;
				combo.SelectedIndex = 0;
			}
			catch (Exception)
			{
				MessageBox.Show("Error al cargar las categorias");
			}
		}

		//Evento de modificación y creacion de artículo
		private void btnModificar_Click(object sender, EventArgs e)
		{
			//Inicia negocio de artiuclos
			ArticuloNegocio articuloNegocio = new ArticuloNegocio();
			
			//Si el modo es agregar, instancia producto vacío
			if (this.esAgregar)
			{
				articulo = new Articulo();
			}

			//cargamos los datos del formulario al objeto
			cargarDatosDeFormulario(articulo);

			if(this.esAgregar) //Agregar Articulo a la base de datos
			{
                
               
				if (btnModificar.Text == "Guardar")
				{
					try
					{
						// Guardar articulo
						articulo.Id = articuloNegocio.agregar(articulo);

						MessageBox.Show("Se Guardó correctamente");

						// Validar si guardó y pasar a modo vista
						this.hayCambios = true;
						this.esAgregar = false;
						this.esModificar = false;

						//Pasar a modo vista
						ModoModificar(this.esAgregar);

						return;
					}
					catch (Exception err)
					{
						MessageBox.Show("Error al guardar el articulo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			else //Modificar Articulo en la base de datos
			{
				if (btnModificar.Text == "Guardar")
				{
                   
					// Validaciones

                    //Verificar si se quiere cancelar
                    DialogResult result = MessageBox.Show("¿Está seguro que quiere modificar el detalle", "Cancelar", MessageBoxButtons.OKCancel);

					if (result == DialogResult.OK)
					{
						//Modificar producto
						if (articuloNegocio.modificar(articulo))
						{
							MessageBox.Show("Se modificó correctamente");

							// Si se guardan correctamente se cierra la ventana
							this.hayCambios = true;
							
							// Validar si guardó y pasar a modo vista
							this.hayCambios = true;
							this.esAgregar = false;
						}
						else
						{
							MessageBox.Show("Error al modificar el articulo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
				}
			}

			this.esModificar = !this.esModificar;
			ModoModificar(this.esModificar);
		}

		private void cargarDatosDeFormulario(Articulo articulo)
		{	
			// Ariticulo
			string precio = txtPrecio.Text.Trim();
			string codigo = txtCodigo.Text.Trim().ToLower();
			string nombre = txtArticulo.Text.Trim();
			string descripcion = txtDescripcion.Text.Trim();

			// Marca
			Marca marca = new Marca();
			marca.Descripcion = cbMarca.Text;
			marca.Id = (int)cbMarca.SelectedValue;
			
			// Categoria
			Categoria categoria = new Categoria();
			categoria.Descripcion = cbCategoria.Text;
			categoria.Id = (int)cbCategoria.SelectedValue;

			// Guardar datos de la ventana en el articulo
			articulo.Codigo = codigo;
			articulo.Precio = Convert.ToDecimal(precio.Replace(".", ","));
			articulo.Nombre = nombre;
			articulo.Descripcion = descripcion;
			articulo.Marca = marca;
			articulo.Categoria = categoria;
		}

		private void btnCancelar_Click(object sender, EventArgs e)
		{
			if (this.esModificar)
			{
				// En Modo Modificar verificamos que se quiera cancelar
				DialogResult result = MessageBox.Show("¿Está seguro que quiere cancelar?", "Cancelar", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

				// Si se cancela, vuelve a Modo Vista
				if (result == DialogResult.OK)
				{
					this.esModificar = false;
					ModoModificar(this.esModificar);
				}
			}
			else
			{
				// En Modo Agregar, cerrar la ventana si se cancela
				this.Close();
			}
		}

		// Borrar la imagen del carrusel actual
		private void btnImagenBorrarActual_Click(object sender, EventArgs e)
		{
			ImagenNegocio imagenNegocio = new ImagenNegocio();

			// Verificar si se quiere cancelar
			DialogResult result = MessageBox.Show("¿Está seguro que quiere borrar la imagen actual?", "Cancelar", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

			if (result == DialogResult.OK)
				if (imagenNegocio.borrar(imagenActual))
				{
					// Remover imagen borrada del objeto
					articulo.Imagenes = articulo.Imagenes.FindAll(x => x.Id != imagenActual);
					
					// Cargamos imagenes
					CargarImagenes();

					// Seteamos controles de las imagenes
					ImagenControl();

					// Ponemos los cambios en true para que la ventana de listado se actualice
					this.hayCambios = true;
				}
		}
		
		// Carrusel, Boton Izquierda
		private void btnAnteriorImagen_Click(object sender, EventArgs e)
		{
			if(imagenActualCarrusel > 0)
			{
				imagenActualCarrusel--;
				
				if (imagenActualCarrusel < articulo.Imagenes.Count - 1)
					pbFlechaDerecha.Visible = true;
				
				imagenActual = articulo.Imagenes[imagenActualCarrusel].Id;
				CargarPictureBox(pbImagenArticulo, articulo.Imagenes[imagenActualCarrusel].UrlImagen);
			}
			
			if(imagenActualCarrusel == 0)
			{
				pbFlechaIzquierda.Visible = false;
			}
		}

		private void btnSiguienteImagen_Click(object sender, EventArgs e)
		{
			if (imagenActualCarrusel < articulo.Imagenes.Count - 1)
			{
				imagenActualCarrusel++;
				
				if (imagenActualCarrusel > 0)
					pbFlechaIzquierda.Visible = true;
				
				imagenActual = articulo.Imagenes[imagenActualCarrusel].Id;
				CargarPictureBox(pbImagenArticulo, articulo.Imagenes[imagenActualCarrusel].UrlImagen);
			}
			
			if(imagenActualCarrusel == articulo.Imagenes.Count - 1)
			{
				pbFlechaDerecha.Visible = false;
			}
		}

		private void rbUrl_CheckedChanged(object sender, EventArgs e)
		{
			ControlGuardarImagen();
		}

		private void rbArchivo_CheckedChanged(object sender, EventArgs e)
		{
			ControlGuardarImagen();
		}

		private void ControlGuardarImagen()
		{	
			if (rbUrl.Checked || rbArchivo.Checked)
			{
				txtImagenUrl.Visible = true;
				btnGuardarImagen.Visible = true;

				if(rbArchivo.Checked)
				{
					btnGuardarImagen.Enabled = true;
					txtImagenUrl.Enabled = false;
					btnGuardarImagen.Text = "Cargar";
					
				}

				if (rbUrl.Checked)
				{
					btnGuardarImagen.Enabled = false;
					txtImagenUrl.Enabled = true;
					btnGuardarImagen.Text = "Guardar";
				}
			}
			else
			{
				txtImagenUrl.Visible = false;
				btnGuardarImagen.Visible = false;
			}
		}

		private void btnGuardarImagen_Click(object sender, EventArgs e)
		{
			//Guardar imagen Desde archivo o link
			if (btnGuardarImagen.Text == "Cargar")
			{
				file = new OpenFileDialog();
				file.Filter = "All|*|Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";

				if (file.ShowDialog() == DialogResult.OK)
				{
					txtImagenUrl.Text = file.FileName;
					try
					{
						CargarPictureBox(pbImagenArticulo, file.FileName);
						btnGuardarImagen.Text = "Guardar";
					}
					catch (Exception)
					{
						MessageBox.Show("La iamgen no pudo ser cargada", "Cargada");
					}
				}
			}
			else // Guardar imagen
			{
				Imagen aux = new Imagen();
				aux.IdArticulo = articulo.Id;
				
				
				if (txtImagenUrl.Text.Contains("http")) // Cargando link
				{
					aux.UrlImagen = txtImagenUrl.Text;
				}
				else // Cargando Archivo
				{
					//Copiar imagen en carpeta del proyecto
					copiarImagen(aux, file);
				}

				ImagenNegocio imagenNegocio = new ImagenNegocio();

				try
				{
					//Guardamos Imagen en la base de datos y luego su Id en el objeto de la imagen
					aux.Id = imagenNegocio.agregar(aux);
					
					if (articulo.Imagenes == null)
					{
						//Buscamos el listado de imágenes de la base de datos y se la agregamos al arituclo
						articulo.Imagenes = imagenNegocio.listar(articulo.Id.ToString());
					}
					else
					{
						//Cargamos imagenes en la lista de imágenes
						articulo.Imagenes.Add(aux);
					}

					MessageBox.Show("La imagen se guardó exitosamente!");
				}
				catch(Exception err)
				{
					MessageBox.Show("La imagen no pudo ser cargada", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				//Regresamos checked a false y vaciamos textbox
				rbArchivo.Checked = false;
				rbUrl.Checked = false;
				txtImagenUrl.Text = "";

				//Reseteamos controles de la imagen
				ControlGuardarImagen();

				//Si contiene imagenes, cargarlas
				CargarImagenes();

				//Seteamos controles de las imagenes
				ImagenControl();
			}
		}

		private void btnEliminar_Click(object sender, EventArgs e)
		{
			ArticuloNegocio articuloNegocio = new ArticuloNegocio();

			//Verificar si se quiere cancelar
			DialogResult result = MessageBox.Show("¿Está seguro que quiere Eliminar el artículo?", "Cancelar", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

			//Borrar imagen
			if (result == DialogResult.OK)
				if (articuloNegocio.eliminar(articulo.Id))
				{
					//Ponemos los cambios en true para que la ventana de listado se actualice
					this.hayCambios = true;

					//Cerramos ventana
					this.Close();
				}
		}

		private void txtImagenUrl_TextChanged(object sender, EventArgs e)
		{
			//Cargamos en el picturebox la imagen del link
			if (txtImagenUrl.Text.Contains("http"))
			{
				CargarPictureBox(pbImagenArticulo, txtImagenUrl.Text);
			}
		}

		private void Listararticulos() // Para validaciones
		{
			ArticuloNegocio articulosNegocio = new ArticuloNegocio();
            listaArticulos = articulosNegocio.listar();
        }

		//Guardar la imagen que se carga en archivos en una carpeta propia del programa
		 private void copiarImagen(Imagen imagen, OpenFileDialog file)
		{
			string imgFolder = rutaImagen + "\\img_articulos\\";
			
			// Si el directorio no existe, se crea
			if (!Directory.Exists(imgFolder))
			{
				Directory.CreateDirectory(imgFolder);
			}

			try
			{
				// Si el archivo ya se encuentra en la carpeta, pregunta si se quiere sobreescribir
				if (File.Exists(imgFolder + file.SafeFileName))
				{
					DialogResult result = MessageBox.Show("El archivo ya existe. ¿Desea Remplazarlo?", "Ya existe", MessageBoxButtons.OKCancel);

					if (result == DialogResult.OK)
					{
						File.Delete(imgFolder + file.SafeFileName);
						File.Copy(file.FileName, imgFolder + file.SafeFileName);

						//Guardar imagen en listado de imagenes dentro del artículo
						imagen.UrlImagen = imgFolder + file.SafeFileName;
					}
				}
				else
				{
					File.Copy(file.FileName, imgFolder + file.SafeFileName);

					//Guardar imagen en listado de imagenes dentro del artículo
					imagen.UrlImagen = imgFolder + file.SafeFileName;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		// Validación de código, si existe o si es un código válido
		private bool Validarcodigo()
        {
            bool formatocodigo = Regex.IsMatch(txtCodigo.Text.Trim(), @"^[A-Za-z]\d{2}$");

            if (!formatocodigo)
            {
				LblAvisoCodigo.Text = "* El formato debe ser de tipo \"A00\"";
				btnModificar.Enabled = false;
                LblAvisoCodigo.Visible = true;
				return false;
            }
            else if (Validarexistenciacodigo()) 
			{
				LblAvisoCodigo.Text = "* El codigo de articulo ya existe";
				btnModificar.Enabled = false;
				LblAvisoCodigo.Visible = true;
				return false;
            }
			else
			{
                btnModificar.Enabled = true;
                LblAvisoCodigo.Visible = false;
				return true;
            }
        }

		// Validar el nombre si existe
		private bool Validarnombre()
		{
			if (Validarexistencianombre())
			{
                LblAvisoNombre.Text = "* El Nombre de articulo ya existe";
                btnModificar.Enabled = false;
                LblAvisoNombre.Visible = true;
				return false;
			}
			else
			{
                btnModificar.Enabled = true;
                LblAvisoNombre.Visible = false;
				return true;
            }
			
		}

		// Verifica que todos los campos estén completos
		private bool Completotodosloscampos()
		{
			if (txtCodigo.Text != string.Empty && txtArticulo.Text != string.Empty && txtDescripcion.Text != string.Empty && txtPrecio.Text != string.Empty)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		// Busca codigo igual en listado de articulos
		private bool Validarexistenciacodigo()
        {
			return listaArticulos.Any(x => x.Codigo.ToUpper() == txtCodigo.Text.ToUpper());
		}

		// Busca nombre igual en listado de articulos
		private bool Validarexistencianombre()
		{
            return listaArticulos.Any(x => x.Nombre.ToUpper() == txtArticulo.Text.ToUpper());
        }
		
		//Valida formato del precio
		private bool ValidarPrecio()
        {
            bool formatoprecio = Regex.IsMatch(txtPrecio.Text.Trim(), @"^\d+(,\d{1,2})?$");

            if (!formatoprecio)
            {
				LblAvisoPrecio.Text = "* Ingrese numeros de hasta 2 decimales \n separados por ,(coma)";
                btnModificar.Enabled = false;
                LblAvisoPrecio.Visible = true;
				return false;
            }
            else
            {
                btnModificar.Enabled = true;
                LblAvisoPrecio.Visible = false;
				return true;
            }
        }
		
        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            if (esAgregar || esModificar)
                Validarcodigo();
			
        }

        private void txtArticulo_TextChanged(object sender, EventArgs e)
        {
			if (esAgregar||esModificar)
				Validarnombre();
        }

        private void txtPrecio_TextChanged(object sender, EventArgs e)
        {
			
			if (esAgregar || esModificar)
				ValidarPrecio();
			
        }
        private void InvisibilizarAvisos()
        {
            LblAvisoCodigo.ForeColor = Color.DarkRed;
            LblAvisoNombre.Visible = false;
            LblAvisoNombre.ForeColor = Color.DarkRed;
            LblAvisoCodigo.Visible = false;
            LblAvisoPrecio.ForeColor = Color.DarkRed;
            LblAvisoPrecio.Visible = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
			if (esAgregar||esModificar)
			{
				if (!Completotodosloscampos())
				{

					btnModificar.Enabled = false;
				}
				else
				{
					btnModificar.Enabled = true;
				}
			}
        }
    }
}
