﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dominio;
using Negocio;

namespace Presentacion
{
	//TODO: Validaciones
	//TODO: Guardar imagen nueva por Archivo y url. Validar cual es cual.
	//TODO: Al guardar imagen, copiar en una nueva carpeta para guardar ruta en base de datos
	//TODO: Estilos de la ventana
	//TODO: Eliminar Articulo

	public partial class AgregarArticulos : Form
	{
		private Articulo articulo = null;

		//Control de Imagenes
		private OpenFileDialog file = null;
		private int imagenActual;
		private int imagenActualCarrusel = 0;
		private string rutaImagen = Path.GetDirectoryName(Directory.GetCurrentDirectory().Replace(@"\bin", "")) ;

		//Banderas de vistas
		private bool esModificar;
		private bool esAgregar;

		//Bandera de cambios en la ventana para cargar lista en el form principal
		public bool hayCambios;

		//Constructor para los casos de vista y modificación de artículo
		public AgregarArticulos(Articulo articulo, bool esModificar = false)
		{
			InitializeComponent();

			if (articulo != null)
			{
				this.articulo = articulo;
			}

			this.esModificar = esModificar;

			//Sete la ventana en modo modificar
			ModoModificar(this.esModificar);

			//Carga los datos del artículo en la ventana
			CargarArticulo();
		}

		//Constructor para el caso de agregar artículo
        public AgregarArticulos(bool esAgregar = true)
		{
			InitializeComponent();

			this.esAgregar = esAgregar;

			ModoAgregar(this.esAgregar);

		}

		//Evento de carga inicial de la ventana
		private void Articulos_Load(object sender, EventArgs e)
		{
			//Carga los combo box de categorías y marcas
			CargarComboBox();

			//Control de guardado de imagen
			ControlGuardarImagen();
		}

		private void ModoModificar(bool esModificar)
		{
			//Habilitar o deshabilitar cada uno de los campos de texto
			txtCodigo.Enabled = esModificar;
			txtArticulo.Enabled = esModificar;
			txtDescripcion.Enabled = esModificar;
			txtPrecio.Enabled = esModificar;
			cbCategoria.Enabled = esModificar;
			cbMarca.Enabled = esModificar;
			btnCancelar.Visible = esModificar;

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

			//Seteamos controles de las imagenes
			ImagenControl();
		}

		//Metodo para cargar las imagenes
		private void CargarImagenes()
		{
			if(articulo.Imagenes?.Count > 0)
			{
				CargarPictureBox(pbImagenArticulo, articulo.Imagenes[0].UrlImagen);
				
				//Guardamos id de la imagen actual por si necesita ser eliminada
				imagenActual = articulo.Imagenes[0].Id;
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
			}
			catch(Exception ex)
			{
				//Si hay un error carga una imagen placeholder
				pb.Load(rutaImagen + "/img/placeholder.png");
			}
		}

		private void ImagenControl()
		{
			//Si hay imagenes, habilita los botones de carrusel
			if (articulo.Imagenes?.Count > 0)
			{
				btnImagenBorrarActual.Visible = true;

				//Si hay mas de una imagen, habilita los botones de carrusel
				if (articulo.Imagenes.Count > 1)
				{
					btnAnteriorImagen.Visible = true;
					btnSiguienteImagen.Visible = true;

					return;
				}
			}
			else
			{
				//Si no hay imagenes, deshabilita los botones de carrusel
				btnImagenBorrarActual.Visible = false;
			}

			btnAnteriorImagen.Visible = false;
			btnSiguienteImagen.Visible = false;
		}

		private void CargarComboBox()
		{
			//Cargar combo box de categoria y marca
			comboBoxMarca(cbMarca);
			comboBoxCategoria(cbCategoria);

			//Formatear los datos de los combo box
			cbMarca.ValueMember = "Id";
			cbMarca.DisplayMember = "Descripcion";
			cbCategoria.ValueMember = "Id";
			cbCategoria.DisplayMember = "Descripcion";

		}

		//Cargar en el combo box las marcas desde la base de datos
		public static void comboBoxMarca(ComboBox combo)
		{
			try
			{
				MarcaNegocio marcaNegocio = new MarcaNegocio();
				List<Marca> listaMarca = marcaNegocio.listar();

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
				CategoriaNegocio categoriaNegocio = new CategoriaNegocio();
				List<Categoria> listaCategoria = categoriaNegocio.listar();

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
				if(btnModificar.Text == "Guardar")
				{
					// Guardar articulo
					if (articuloNegocio.agregar(articulo))
					{
						MessageBox.Show("Se Guardó correctamente");

						// Validar si guardó y pasar a modo vista
						this.hayCambios = true;
						this.esAgregar = false;
						//this.esModificar = false;
						//ModoModificar(this.esAgregar);
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
							//this.esModificar = false;
							//ModoModificar(this.esAgregar);
						}
						else
						{
							MessageBox.Show("Error al modificar");
						}
					}
				}
			}

			this.esModificar = !this.esModificar;
			ModoModificar(this.esModificar);
		}

		private void cargarDatosDeFormulario(Articulo articulo)
		{
			//TODO: Validaciones de los campos
			
			//Ariticulo
			string precio = txtPrecio.Text.Trim();
			string codigo = txtCodigo.Text.Trim();
			string nombre = txtArticulo.Text.Trim();
			string descripcion = txtDescripcion.Text.Trim();

			// Marca
			Marca marca = new Marca();
			marca.Descripcion = cbMarca.Text;
			marca.Id = (int)cbMarca.SelectedValue;

			//Categoria
			Categoria categoria = new Categoria();
			categoria.Descripcion = cbCategoria.Text;
			categoria.Id = (int)cbCategoria.SelectedValue;

			//Guardar datos de la ventana en el articulo
			articulo.Codigo = codigo;
			articulo.Precio = Convert.ToDouble(precio.Replace(".", ","));
			articulo.Nombre = nombre;
			articulo.Descripcion = descripcion;
			articulo.Marca = marca;
			articulo.Categoria = categoria;
		}

		private void btnCancelar_Click(object sender, EventArgs e)
		{
			if (this.esModificar)
			{
				//En Modo Modificar verificamos que se quiera cancelar
				DialogResult result = MessageBox.Show("¿Está seguro que quiere cancelar", "Cancelar", MessageBoxButtons.OKCancel);

				//Si se cancela, vuelve a Modo Vista
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

		private void btnImagenBorrarActual_Click(object sender, EventArgs e)
		{
			ImagenNegocio imagenNegocio = new ImagenNegocio();

			//Verificar si se quiere cancelar
			DialogResult result = MessageBox.Show("¿Está seguro que quiere borrar la imagen actual?", "Cancelar", MessageBoxButtons.OKCancel);

			if (result == DialogResult.OK)
				if (imagenNegocio.borrar(imagenActual))
				{
					//Remover imagen borrada del objeto
					articulo.Imagenes = articulo.Imagenes.FindAll(x => x.Id != imagenActual);
					
					//Cargamos imagenes
					CargarImagenes();

					//Seteamos controles de las imagenes
					ImagenControl();

					//Ponemos los cambios en true para que la ventana de listado se actualice
					this.hayCambios = true;
				}
		}

		private void btnAnteriorImagen_Click(object sender, EventArgs e)
		{
			if(imagenActualCarrusel > 0)
			{
				imagenActualCarrusel--;
				imagenActual = articulo.Imagenes[imagenActualCarrusel].Id;
				CargarPictureBox(pbImagenArticulo, articulo.Imagenes[imagenActualCarrusel].UrlImagen);
			}
		}

		private void btnSiguienteImagen_Click(object sender, EventArgs e)
		{
			if (imagenActualCarrusel < articulo.Imagenes.Count - 1)
			{
				imagenActualCarrusel++;
				imagenActual = articulo.Imagenes[imagenActualCarrusel].Id;
				CargarPictureBox(pbImagenArticulo, articulo.Imagenes[imagenActualCarrusel].UrlImagen);
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
			int y = 359;
			
			if (rbUrl.Checked || rbArchivo.Checked)
			{
				y = 395;

				txtImagenUrl.Visible = true;
				btnGuardarImagen.Visible = true;

				if(rbArchivo.Checked)
				{
					txtImagenUrl.Enabled = false;
					btnGuardarImagen.Text = "Cargar";

				}

				if (rbUrl.Checked)
				{
					txtImagenUrl.Enabled = true;
					btnGuardarImagen.Text = "Guardar";
				}

			}
			else
			{
				txtImagenUrl.Visible = false;
				btnGuardarImagen.Visible = false;
			}

			btnCancelar.Location = new Point(btnCancelar.Location.X, y);
			btnEliminar.Location = new Point(btnEliminar.Location.X, y);
			btnModificar.Location = new Point(btnModificar.Location.X, y);
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
						MessageBox.Show("La iamgen no pudo ser cargada");
					}
				}
			}
			else // Guardar imagen
			{
				Imagen aux = new Imagen();
				aux.IdArticulo = articulo.Id;
				
				
				if (txtImagenUrl.Text.Contains("http")) //LINK
				{
					aux.UrlImagen = txtImagenUrl.Text;
				}
				else // ARCHIVO
				{
					aux.UrlImagen = file.FileName;
				}

				ImagenNegocio imagenNegocio = new ImagenNegocio();
				
				if (imagenNegocio.agregar(aux))
				{
					//Cargamos imagenes
					articulo.Imagenes.Add(aux);
					MessageBox.Show("La imagen se guardó exitosamente!");

				}
				else
				{
					MessageBox.Show("La imagen no pudo ser cargada");
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
	}
}
