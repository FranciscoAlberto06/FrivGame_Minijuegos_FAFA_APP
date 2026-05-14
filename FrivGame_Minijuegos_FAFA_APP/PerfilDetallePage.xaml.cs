using BModelosFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PerfilDetallePage : ContentPage
{

	Perfil perfilActual;

	public PerfilDetallePage(Perfil perfil)
	{
		InitializeComponent();
        // Esto permite que el XAML lea los datos de tu objeto Perfil
        System.Diagnostics.Debug.WriteLine($"Perfil: {perfil.NombreUsuario} - {perfil.Nivel} ");
        perfil.AvatarUrl = "avatar1.png";
        this.BindingContext = perfil;
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        // Cierra la página modal
        await Navigation.PopModalAsync();
    }
    private async void OnEditarAvatarClicked(object sender, TappedEventArgs e)
    {
        // Aquí irá la lógica de editar avatar
        await DisplayAlert("Editar Avatar", "Próximamente", "OK");
    }

    private async void OnEditarNombreClicked(object sender, TappedEventArgs e)
    {
        // Aquí irá la lógica de editar nombre
        string nuevoNombre = await DisplayPromptAsync("Editar Nombre", "Introduce tu nuevo nombre:", "Guardar", "Cancelar", placeholder: "Nombre de usuario");

        if (!string.IsNullOrWhiteSpace(nuevoNombre))
        {
            // Actualizar nombre
        }
    }

    private async void OnCambiarPasswordClicked(object sender, EventArgs e)
    {
        string actual = TxtPassActual.Text;
        string nueva1 = TxtPassNueva1.Text;
        string nueva2 = TxtPassNueva2.Text;

        // 1. Validaciones básicas
        if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(nueva1))
        {
            await DisplayAlert("Error", "Rellena todos los campos", "OK");
            return;
        }

        if (nueva1 != nueva2)
        {
            await DisplayAlert("Error", "Las nuevas contraseñas no coinciden", "OK");
            return;
        }

        // 2. Aquí llamarías a tu API para actualizar
        // bool exito = await ApiService.CambiarPassword(perfilActual.IdUsuario, actual, nueva1);

        await DisplayAlert("Éxito", "Contraseña actualizada correctamente", "OK");
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool respuesta = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que quieres salir?", "Sí", "No");

        if (respuesta)
        {
            // 1. Limpiar datos de navegación/preferencias si los usas
            // Preferences.Clear(); 

            // 2. Volver a la página de Login
            // Usamos Navigation.PopModalAsync para cerrar el perfil y luego redirigir
            //await Navigation.PushAsync(new PageInicioSesion());
        }
    }

}