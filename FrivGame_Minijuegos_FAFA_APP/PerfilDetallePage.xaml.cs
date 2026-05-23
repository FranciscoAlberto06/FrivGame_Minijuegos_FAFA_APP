using BGestionFAFA;
using BModelosFAFA;

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PerfilDetallePage : ContentPage
{

    // Lista de imágenes disponibles en tu carpeta Resources/Images
    private List<string> MisFotos { get; set; } = new List<string>
    {
        "avatar1.png", "avatar2.png", "avatar3.png", "avatar4.png"
    };
    int contadorLogros = 0;
    private Perfil perfilActual;

    public PerfilDetallePage(Perfil perfil)
	{
		InitializeComponent();
       
        this.BindingContext = perfil;
        perfilActual = perfil;
        ColAvatares.ItemsSource = MisFotos;
        CargarLogros(perfil);
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
                    perfilActual.NombreUsuario = nuevoNombre;
                    await ApiRestFAFA.ModificarNombre(perfilActual);

                    // Actualizamos el binding visual
                    this.BindingContext = null;
                    this.BindingContext = perfilActual;



                    await DisplayAlert("Éxito", "Nombre actualizado correctamente", "OK");

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

    private async void OnAvatarSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            string avatarElegido = e.CurrentSelection.FirstOrDefault()?.ToString();
            if (avatarElegido == null) return;

            perfilActual.AvatarUrl = avatarElegido;

            // Llamamos a la API para actualizar el avatar
            await ApiRestFAFA.ModificarAvatar(perfilActual);

            // Actualizamos visual
            this.BindingContext = null;
            this.BindingContext = perfilActual;

            // Cerramos el panel
            await PanelAvatares.TranslateTo(0, 400, 250, Easing.CubicIn);
            CapaFondo.IsVisible = false;

            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async void OnCambiarPasswordClicked(object sender, EventArgs e)
    {
        string passIntento = TxtPassActual.Text;
        string nueva1 = TxtPassNueva1.Text;
        string nueva2 = TxtPassNueva2.Text;

        try
        {
            // 1. Validaciones basicas
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                if (string.IsNullOrEmpty(passIntento) || string.IsNullOrEmpty(nueva1))
                {

                    throw new Exception("No puede ser nulas o vacias");

                }
                else if (nueva1 != nueva2)
                {
                    throw new Exception("Las nuevas contraseñas no coinciden");
                }
                else if (nueva1.Length <= 5)
                {
                    throw new Exception("Las nueva contraseña  debe tener el menos 6 caracteres");
                }
                else
                {
                    // 2. Aquí llamarías a tu API para actualizar

                    // Validamos la contraseña si es esa o no
                    await ApiRestFAFA.VerificarPassword(perfilActual.IdUsuario, passIntento);


                    // Si todo va bien, actualizamos la contraseña en la nube y localmente
                    
                    
                    bool exito = await ApiRestFAFA.CambiarPassword(perfilActual.IdUsuario, nueva1);

                    if (exito)
                    {
                        

                        await DisplayAlert("Éxito", "Contraseña actualizada correctamente. Por seguridad redigiremos al inicio de sesion. Muchas gracias por su paciencia", "OK");

                        // Recargamos entrys PUTO
                        TxtPassActual.Text = "";
                        TxtPassNueva1.Text = "";
                        TxtPassNueva2.Text = "";

                        // Cerramos lo que tenemos abierto
                        await Navigation.PopModalAsync();

                        // En la app shell tenemos el login como la pgian raiz llamamemos a este y no a page Inicio sesion porque da problema mostrando el titulo de la paginga y arruina el diseño
                        Application.Current.MainPage = new AppShell();

                    }   

                }
            }
            else
            {
                throw new Exception("Necesitas internet para modificar tu contraseña");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ha ocurrido un error: {ex.Message}", "OK");
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

    private void CargarLogros(Perfil perfil)
    {
        //1. Sacamos todos los logros y cuales tiene desbloqueados el perfil
        List<Logro> todosLosLogros = ApiSQLiteFAFA.ExtraerTodosLosLogros();
        List<int> logrosDesbloqueados = ApiSQLiteFAFA.ExtraerIdsLogrosPorPerfil(perfil.PerfilUid);

        listaLogros.Children.Clear();

        // 2. Bucle que recorre todos los logros
        foreach (Logro logro in todosLosLogros.OrderByDescending(l => logrosDesbloqueados.Contains(l.IdLogro)))
        {
            // 3. Creamos una tarjeta por cada logros con mas o menos opacidad segun si lo tiene desbloqeuado
            bool desbloqueado = logrosDesbloqueados.Contains(logro.IdLogro);

            // Contamos lo logros para mostrarlo en las estadisticas del perfil
            if (desbloqueado) contadorLogros++;

            Border tarjeta = new Border
            {
                WidthRequest = 150,
                HeightRequest = 150,
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                // Si tiene logro 0 opacidad, y si lo tiene bajamos la opacidad
                Opacity = desbloqueado ? 1.0 : 0.3,
                // Para redondearlo
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 15 }
            };

            VerticalStackLayout contenido = new VerticalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            contenido.Children.Add(new Image
            {
                Source = "trofeo_global.png",
                HeightRequest = 50,
                WidthRequest = 50,
                HorizontalOptions = LayoutOptions.Center
            });

            contenido.Children.Add(new Label
            {
                Text = logro.Nombre,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });

            contenido.Children.Add(new Label
            {
                Text = $"+{logro.XpPremio} XP",
                TextColor = Colors.Crimson,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            });

            tarjeta.Content = contenido;
            listaLogros.Children.Add(tarjeta);
        }
        lLogrosNum.Text = contadorLogros.ToString();
    }


}