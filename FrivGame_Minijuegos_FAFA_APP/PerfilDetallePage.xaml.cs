using BGestionFAFA;
using BModelosFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PerfilDetallePage : ContentPage
{

    // Lista de imágenes disponibles en tu carpeta Resources/Images
    public List<string> MisFotos { get; set; } = new List<string>
    {
        "avatar1.png", "avatar2.png", "avatar3.png", "avatar4.png"
    };

    public PerfilDetallePage(Perfil perfil)
	{
		InitializeComponent();
       
        this.BindingContext = perfil;
        ColAvatares.ItemsSource = MisFotos;
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        // Cierra la página modal
        await Navigation.PopModalAsync();
    }
    private async void OnEditarAvatarClicked(object sender, TappedEventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {


            // Mostramos con una animacion el panel de logos
            CapaFondo.IsVisible = true;
            await PanelAvatares.TranslateTo(0, 0, 300, Easing.CubicOut);

            // 
            // ApiRestFAFA.ModificarAvatar(ColAvatares.SelectedItem);
        }
        else
        {
            await DisplayAlert("Sin conexión", "Necesitas internet para cambiar tu avatar", "OK");
        }
    }

    private async void OnCancelarSeleccionClicked(object sender, EventArgs e)
    {
        // Animación de bajada
        await PanelAvatares.TranslateTo(0, 400, 250, Easing.CubicIn);
        CapaFondo.IsVisible = false;
    }


    private async void OnEditarNombreClicked(object sender, TappedEventArgs e)
    {
        try
        {
            // Si teneemos conexion a internet, permitimos editar el nombre, sino mostramos un mensaje de error
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                // Solicitamos el nuevo nombre al usuario 
                string nuevoNombre = await DisplayPromptAsync("Editar Nombre", "Introduce tu nuevo nombre:", "Guardar", "Cancelar", placeholder: "Nombre de usuario");

                // Si no es nulo o vacio, actualizamos el nombre del perfil
                if (!string.IsNullOrWhiteSpace(nuevoNombre))
                {
                    // TODO: Implementar la lógica para actualizar el nombre en la nube y localmente
                }
                else
                {
                    throw new Exception("El nombre no puede estar vacío");
                }
            }
            else // Sino hay internet, mostramos un mensaje de error
            {
                
                throw new Exception("Necesitas internet para cambiar tu nombre");
            }



        } catch (Exception error)
        {
            await DisplayAlert("UPS! Ha habido un error", $"{error}", "OK");

        }


    }

    private void OnAvatarSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            // Sacamos el perfil actual 
            Perfil perfil = (Perfil)this.BindingContext;
            // Sacamos el nombre del avata selecionado
            perfil.AvatarUrl = (string)e.CurrentSelection.FirstOrDefault();

            // TODO: Llamar a api para actualizar el avatar en la nube y localmente
            //ApiRestFAFA.ActualizarPerfil(perfil);

            // Forzar refresco visual
            this.BindingContext = null;
            this.BindingContext = perfil;

            OnPropertyChanged();


            // Deseleccionar para la próxima vez
            ((CollectionView)sender).SelectedItem = null;
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
        }
        else if (nueva1 != nueva2)
        {
            await DisplayAlert("Error", "Las nuevas contraseñas no coinciden", "OK");
        }
        else 
        {
            // 2. Aquí llamarías a tu API para actualizar

            // Validamos la contraseña si es esa o no

            // Si todo va bien, actualizamos la contraseña en la nube y localmente

            // bool exito = await ApiService.CambiarPassword(perfilActual.IdUsuario, actual, nueva1);

            await DisplayAlert("Éxito", "Contraseña actualizada correctamente", "OK");

        }
    }


    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool respuesta = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que quieres salir?", "Sí", "No");

        if (respuesta)
        {
            // Cerramos lo que tenemos abierto
            await Navigation.PopModalAsync();

            // En la app shell tenemos el login como la pgian raiz llamamemos a este y no a page Inicio sesion porque da problema mostrando el titulo de la paginga y arruina el diseño
            Application.Current.MainPage = new AppShell();

        }
    }

}