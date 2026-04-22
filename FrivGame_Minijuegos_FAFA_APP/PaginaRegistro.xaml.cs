using BGestionFAFA;
using BModelosFAFA;
using BModelosSQLFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PaginaRegistro : ContentPage
{
	public PaginaRegistro()
	{
		InitializeComponent();
	}

    private void clickRegistrar(object sender, EventArgs e)
    {
        int ususActualId;
        bool errorDetectado = false;
        try
        {
            #region  GUARDADO LOCAL

            // 1. Validacion de campos vacios
            if (string.IsNullOrWhiteSpace(eEmail.Text) ||
                string.IsNullOrWhiteSpace(eNombreUsuario.Text) ||
                string.IsNullOrWhiteSpace(ePassword.Text))
            {
                throw new Exception("Todos los campos son obligatorios.");
            }

            // 2 Comprobacion y creacion de usuario
            // 2.1. Comprobamos que no exista ya el usuario antes de hacer nada
            ApiSQLiteFAFA.ComprobarSiExisteUsuario(eEmail.Text);

            // 2.2. Creamos el usuario a agregar si todo a salido bien en la comprobacion
            Usuario usuNuevo = new Usuario(eEmail.Text);

            // 2.3. Metemos el usuario en SQLite y ademas extreamos su id
            ususActualId = ApiSQLiteFAFA.InsertarUsuarioYDevolverID(usuNuevo);

            // 3 Comprobacion y creacion de perfil
            // 3.1. Comprobamos el nombre del perfil no exista
            ApiSQLiteFAFA.ComprobarSiExisteNombreDePerfil(eNombreUsuario.Text);

            // 3.2. Creamos el perfil
            Perfil perfilNuevo = new Perfil(ususActualId, eNombreUsuario.Text,null ); // TODO: Configurar avatar a elegir ahora mismo null 

            // 3.3. Guardamos el perfil
            ApiSQLiteFAFA.InsertarPerfil(perfilNuevo);

            // 4 Guardamos contraseña ocultada mediante un id que se hace a partir del id de usuario
            ApiSQLiteFAFA.GuardarContrasenaOculta(ususActualId ,ePassword.Text);

            #endregion

            #region GUARDADO EN LA NUBE
            #endregion

            // 4. Mostramos un mensaje de estado al usuario 
            lError.IsVisible = true;


        }
        catch (Exception ex) 
        {
            lError.TextColor = Colors.Red;
            lError.Text = ex.Message;
            errorDetectado = true;
        }
        finally
        {
            // Comprobamos si ha habido alguno error
            if (!errorDetectado) 
            {
                // Si todo fue bien limpiamos entrys
                eNombreUsuario.Text = "";
                eEmail.Text = "";
                ePassword.Text = "";

                // Mostramos mensaje de que todo fue bien
                lError.TextColor = Colors.Green;
                lError.Text = "Usuario Creado Correctamente";
                
            }
        }

    }

    private async void VolverAlInicio(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new PageInicioSesion());
    }
}