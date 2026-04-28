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

    private async void clickRegistrar(object sender, EventArgs e)
    {
        int ususActualId;
        bool errorDetectado = false;
        try
        {

            // 1. VALIDACIÓN INICIAL
            if (string.IsNullOrWhiteSpace(eEmail.Text) || string.IsNullOrWhiteSpace(eNombreUsuario.Text) || string.IsNullOrWhiteSpace(ePassword.Text))
                throw new Exception("Todos los campos son obligatorios.");

            // 2. COMPROBAR CONEXIÓN (Vital si el registro depende de la nube)
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                throw new Exception("Se requiere conexión a internet para registrarse.");

            // 3. GUARDADO EN LA NUBE (Aiven asigna el ID Real)
            // Insertamos el usuario y recuperamos el ID que MySQL generó automáticamente
            int idRealAiven = await ApiAivenFAFA.InsertarUsuarioEnNube(eEmail.Text, eNombreUsuario.Text, ePassword.Text);

            if (idRealAiven > 0)
            {
                // 4. CREAR PERFIL EN LA NUBE
                // Ahora usamos el ID que nos dio Aiven, así no fallará la Foreign Key
                string nuevoUid = Guid.NewGuid().ToString();
                Perfil perfilParaNube = new Perfil(idRealAiven, eNombreUsuario.Text, null) { PerfilUid = nuevoUid };

                await ApiAivenFAFA.InsertarPerfilDirectoEnNube(perfilParaNube);

                // 5. SINCRONIZACIÓN HACIA DE(SQLite copia a Aiven)
                await ApiAivenFAFA.CargarDatosNuevosDesdeAiven(FileSystem.AppDataDirectory);

                // 6. GUARDAR CONTRASEÑA SEGURA (SecureStorage local)
                ApiSQLiteFAFA.GuardarContrasenaOculta(idRealAiven, ePassword.Text);


            }


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