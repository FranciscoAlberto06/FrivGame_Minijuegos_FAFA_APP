namespace FrivGame_Minijuegos_FAFA_APP;

public partial class AdivinaPalabra : ContentPage
{
    string palabraSecreta = "MAUI";
    char[] palabraMostrada;
    int intentos = 6;

    public AdivinaPalabra()
    {
        InitializeComponent();
        IniciarJuego();
    }

    void IniciarJuego()
    {
        palabraSecreta = "MAUI"; // Puedes cambiarla
        palabraMostrada = new string('_', palabraSecreta.Length).ToCharArray();
        intentos = 6;

        ActualizarPantalla();
    }

    void ActualizarPantalla()
    {
        PalabraLabel.Text = string.Join(" ", palabraMostrada);
        IntentosLabel.Text = $"Intentos restantes: {intentos}";
    }

    void OnProbarLetraClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(LetraEntry.Text))
            return;

        char letra = char.ToUpper(LetraEntry.Text[0]);
        LetraEntry.Text = "";

        bool acierto = false;

        for (int i = 0; i < palabraSecreta.Length; i++)
        {
            if (palabraSecreta[i] == letra)
            {
                palabraMostrada[i] = letra;
                acierto = true;
            }
        }

        if (!acierto)
        {
            intentos--;
        }

        ActualizarPantalla();
        VerificarFinJuego();
    }

    void VerificarFinJuego()
    {
        if (!palabraMostrada.Contains('_'))
        {
            DisplayAlert("¡Ganaste!", "Has adivinado la palabra 🎉", "OK");
        }
        else if (intentos <= 0)
        {
            DisplayAlert("Perdiste", $"La palabra era {palabraSecreta}", "OK");
        }
    }

    void OnReiniciarClicked(object sender, EventArgs e)
    {
        IniciarJuego();
    }
}