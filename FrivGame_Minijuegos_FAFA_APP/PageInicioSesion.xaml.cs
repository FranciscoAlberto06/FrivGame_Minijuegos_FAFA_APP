

namespace FrivGame_Minijuegos_FAFA_APP;

public partial class PageInicioSesion : ContentPage
{
	public PageInicioSesion()
	{
		InitializeComponent();
		AnimacionInicio();
	}

    private async void AnimacionInicio()
    {
        // 1. ESTADO INICIAL
        LoginContainer.Opacity = 0;
        LoginContainer.IsVisible = false;

        LogoImage.Scale = 0.5; // Empezamos un poco mas grande para que no sea desde la nada
        LogoImage.Opacity = 0;

        // 2. ESPERAR UN SEGUNDO
        await Task.Delay(1000);

        // 3. APARECER Y REBOTAR
        // Primero le damos opacidad para que se ve
        await LogoImage.FadeTo(1, 200);

        // Despues lo agrandomos un poco más de lo normal 
        await LogoImage.ScaleTo(1.9, 400);

        // Y despues lo volvemos a su tamaño normal para dar un efecto de rebote
        await LogoImage.ScaleTo(1.0, 400);

        // 4. SE PARA OTRO SEGUNDO
        await Task.Delay(1000);

        // 5. DESAPARECER LENTAMENTE
        await LogoImage.FadeTo(0, 600);
        await SplashContainer.FadeTo(0, 400);

        SplashContainer.IsVisible = false;

        // 6. MOSTRAR EL LOGIN
        LoginContainer.IsVisible = true;
        await LoginContainer.FadeTo(1, 800);
    }

    private async void botonInico(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MenuJuegos());
    }
}