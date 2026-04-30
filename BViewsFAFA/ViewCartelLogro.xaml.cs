
namespace BViewsFAFA;

public partial class ModeloCartelLogro : ContentView
{
    private bool mensajeLogroActivo = false;
    private Queue<(string nombre, int xp)> cola = new();

    public ModeloCartelLogro()
	{
		InitializeComponent();
	}

    public async Task MostrarLogro(string nombreLogro, int xp)
    {
        // Metemos en la cola el logro a mostrar 
        // Sino puede entrar en la colar por lo que mensajeactivo estara en true pues se quedara esperando a que pueda seguir por el if
        cola.Enqueue((nombreLogro, xp));


        // Mientras esta un logo activo el otro no va a poder mostrarse, asi evitamos que se amontonen los mensajes de logro
        if (!mensajeLogroActivo)
        {
            mensajeLogroActivo = true;

            // Si hay alguien en la cola lo mostramos
            while (cola.Count > 0)
            {
                //dequeue saca el primer elemento de la cola y lo devuelve, ademas de eliminarlo de la cola
                (string nombre, int premioXp) = cola.Dequeue();

                // 1. Preparamos los textos
                lblNombreLogro.Text = nombre;
                lblXpLogro.Text = $"+{premioXp} XP";
                bLogro.IsVisible = true;
                bLogro.TranslationX = 60;

                // 2. Animacion de entrada
                await Task.WhenAll(
                    bLogro.FadeTo(1, 350),
                    bLogro.TranslateTo(0, 0, 350, Easing.CubicOut)
                );

                // 3. Barra de progreso animada
                await barraProgreso.ScaleXTo(0, 2500, Easing.Linear);

                // 4. Animacion de salida
                await Task.WhenAll(
                    bLogro.FadeTo(0, 350),
                    bLogro.TranslateTo(60, 0, 350, Easing.CubicIn)
                );

                // 5. Reset para el siguiente logro
                bLogro.IsVisible = false;
                bLogro.TranslationX = 0;
                barraProgreso.ScaleX = 1;
            }
            // Abrimos la puerta al siguiente logro
            mensajeLogroActivo = false;

        }
        
    }
}