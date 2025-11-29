using System;
using System.Collections.Generic;
//AUMENTAR EL USO DE ESTE workspace
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Peluqueria
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataClasseUserDataContext db;
        public MainWindow()
        {
            InitializeComponent();

            try
            {

                string conStr = ConfigurationManager.ConnectionStrings["Peluqueria.Properties.Settings.BDPeluqueriaConnectionString"].ConnectionString;

                db = new DataClasseUserDataContext(conStr);

                CargarUsuariosListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar o inicializar el DataContext: {ex.Message}", "Error de Conexión");
                // Es bueno cerrar la aplicación si la conexión falla al inicio
                this.Close();
            }
        }

        // Método para cargar los usuarios en el ListBox
        private void CargarUsuariosListBox()
        {
            try
            {
                lstBxUsuarios.Items.Clear();

                // Consulta LINQ: Obtener todos los usuarios de la tabla
                var usuariosLogins = from u in db.usuario
                                     select u.usuario_login;

                foreach (var login in usuariosLogins)
                {
                    lstBxUsuarios.Items.Add(login);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error");
            }
        }

        // ==========================================
        // MÉTODOS CRUD (CREAR, LEER, ACTUALIZAR, BORRAR)
        // ==========================================

        private void btnAceptar_Click(object sender, RoutedEventArgs e) // Crear/Añadir Usuario
        {
            try
            {
                string nuevoLogin = txtNombre.Text.Trim();
                string nuevaContrasena = pwdContra.Password.Trim();

                if (string.IsNullOrWhiteSpace(nuevoLogin) || string.IsNullOrWhiteSpace(nuevaContrasena))
                {
                    MessageBox.Show("El Login y la Contraseña son obligatorios.", "Advertencia");
                    return;
                }

                // Crear nueva instancia de Usuario
                usuario nuevoUsuario = new usuario()
                {
                    // ⚠️ AJUSTE CLAVE: Asignar un id_empleado existente (ej. el admin con ID 6)
                    id_empleado = 6,

                    usuario_login = nuevoLogin,
                    contrasena = nuevaContrasena,
                    cargo = "Empleado"
                };

                // Insertar y guardar cambios
                db.usuario.InsertOnSubmit(nuevoUsuario);
                db.SubmitChanges();

                MessageBox.Show($"Usuario '{nuevoUsuario.usuario_login}' registrado correctamente.", "Éxito");

                // Limpiar y recargar
                txtNombre.Clear();
                pwdContra.Clear();
                CargarUsuariosListBox();
            }
            catch (System.Data.SqlClient.SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Error: El nombre de usuario (Login) ya existe.", "Error de DB");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e) // Actualizar Contraseña
        {
            if (lstBxUsuarios.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un usuario para actualizar.", "Advertencia");
                return;
            }

            string loginSeleccionado = lstBxUsuarios.SelectedItem.ToString();
            string nuevaContrasena = pwdContra.Password.Trim();

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                MessageBox.Show("La contraseña no puede estar vacía.", "Advertencia");
                return;
            }

            try
            {
                // Buscar el objeto Usuario a actualizar
                usuario usuarioActualizar = db.usuario
                    .FirstOrDefault(u => u.usuario_login == loginSeleccionado);

                if (usuarioActualizar != null)
                {
                    // Modificar la contraseña
                    usuarioActualizar.contrasena = nuevaContrasena;

                    // Enviar los cambios
                    db.SubmitChanges();
                    MessageBox.Show($"Contraseña del usuario '{loginSeleccionado}' actualizada.", "Éxito");

                    // Limpiar campos
                    txtNombre.Clear();
                    pwdContra.Clear();
                    // No es necesario recargar el ListBox, solo se actualizó un valor interno.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error");
            }
        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e) // Borrar Usuario
        {
            if (lstBxUsuarios.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un usuario para borrar.", "Advertencia");
                return;
            }

            string loginSeleccionado = lstBxUsuarios.SelectedItem.ToString();

            if (MessageBox.Show($"¿Está seguro de borrar al usuario '{loginSeleccionado}'?", "Confirmar borrado", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // Buscar el objeto Usuario a eliminar
                    usuario usuarioEliminar = db.usuario
                        .FirstOrDefault(u => u.usuario_login == loginSeleccionado);

                    if (usuarioEliminar != null)
                    {
                        // Eliminar y enviar cambios
                        db.usuario.DeleteOnSubmit(usuarioEliminar);
                        db.SubmitChanges();

                        MessageBox.Show($"Usuario '{loginSeleccionado}' eliminado de la DB.", "Éxito");

                        // Recargar la lista y limpiar
                        CargarUsuariosListBox();
                        txtNombre.Clear();
                        pwdContra.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al borrar: {ex.Message}", "Error");
                }
            }
        }

        // ==========================================
        // MANEJO DE SELECCIÓN Y BÚSQUEDA
        // ==========================================

        private void lstBxUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Cargar datos al seleccionar en la lista
            if (lstBxUsuarios.SelectedValue != null)
            {
                string loginSeleccionado = lstBxUsuarios.SelectedValue.ToString();

                // Buscar el usuario completo en la base de datos
                usuario usuarioSeleccionado = db.usuario
                    .FirstOrDefault(u => u.usuario_login == loginSeleccionado);

                if (usuarioSeleccionado != null)
                {
                    // Mostrar datos
                    txtNombre.Text = usuarioSeleccionado.usuario_login;
                    pwdContra.Password = usuarioSeleccionado.contrasena;


                }
            }
        }
        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string login = txtNombre.Text.Trim();
            string password = pwdContra.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingrese el Login y la Contraseña para intentar ingresar.", "Faltan Datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Consulta LINQ: Buscar el usuario que coincida con el login Y la contraseña
                usuario usuarioEncontrado = db.usuario
                    .FirstOrDefault(u => u.usuario_login == login && u.contrasena == password);

                if (usuarioEncontrado != null)
                {
                    // INGRESO EXITOSO
                    MessageBox.Show($"¡Bienvenido, {usuarioEncontrado.usuario_login}!\nCargo: {usuarioEncontrado.cargo}",
                                    "Ingreso Exitoso",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    // Aquí es donde normalmente abrirías la siguiente ventana (ej. el menú principal)
                    // Por simplicidad, solo limpiamos los campos.
                    txtNombre.Clear();
                    pwdContra.Clear();
                }
                else
                {
                    // INGRESO FALLIDO
                    MessageBox.Show("Login o contraseña incorrectos.",
                                    "Fallo de Autenticación",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al intentar ingresar: {ex.Message}", "Error de DB");
            }
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}