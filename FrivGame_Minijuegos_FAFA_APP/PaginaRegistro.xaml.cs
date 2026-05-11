using BGestionFAFA;
using BModelosFAFA;
using BModelosSQLFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PaginaRegistro : ContentPage
{
    string avatarSeleccionado = "";

    public PaginaRegistro()
	{
		InitializeComponent();
        CargarAvatares();
    }

    private void CargarAvatares()
    {
        listaAvatares.ItemsSource = new List<string>
        {
            "avatar1.png",
            "avatar2.png",
            "avatar3.png",
            "avatar4.png",
         
        };
    }

    private void OnAvatarSeleccionado(object sender, SelectionChangedEventArgs e)
    {

        avatarSeleccionado = (string)e.CurrentSelection.FirstOrDefault();

    }

    private async void clickRegistrar(object sender, EventArgs e)
    {
        bool errorDetectado = false;
        try
        {

            // 1. VALIDACIÓN INICIAL
            if (string.IsNullOrEmpty(avatarSeleccionado))
                throw new Exception("Todos los campos son obligatorios.");
            

            // 2. COMPROBAR CONEXIÓN 
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Se requiere conexión a internet para registrarse.");

            // 3. GUARDADO EN LA NUBE 
            // Insertamos el usuario y recuperamos el ID que MySQL genero automáticamente
            int idRealAiven = await ApiRestFAFA.InsertarUsuarioEnNube(eEmail.Text, eNombreUsuario.Text, ePassword.Text);

            if (idRealAiven > 0)
            {
                // 4. CREAR PERFIL EN LA NUBE
                // Ahora usamos el ID que nos dio Aiven, así no fallara nuestra Foreign Key
                string nuevoUid = Guid.NewGuid().ToString();
                Perfil perfilParaNube = new Perfil(idRealAiven, eNombreUsuario.Text, avatarSeleccionado) { PerfilUid = nuevoUid };

                await ApiRestFAFA.InsertarPerfilDirectoEnNube(perfilParaNube);

                // 5. SINCRONIZACIÓN DE(SQLite copia a Aiven)
                await ApiRestFAFA.CargarDatosDesdeApi(FileSystem.AppDataDirectory);

                // 6. GUARDAR CONTRASEÑA SEGURA (SecureStorage local)
                ApiSQLiteFAFA.GuardarContrasenaOculta(idRealAiven, ePassword.Text);


            }


        }
        catch (Exception ex) 
        {
            lError.IsVisible = true;
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
                lError.IsVisible = true;
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