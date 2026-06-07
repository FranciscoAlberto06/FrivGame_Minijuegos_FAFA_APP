🎮 FrivGame — Plataforma de Minijuegos Multijugador

<img width="1575" height="660" alt="image" src="https://github.com/user-attachments/assets/6751d170-aee1-4feb-b060-1c8ae016c157" />
<img width="1918" height="921" alt="image" src="https://github.com/user-attachments/assets/250a703c-55ae-4068-a153-c3649444a598" />
<img width="745" height="971" alt="image" src="https://github.com/user-attachments/assets/a423d196-0f39-4519-b652-606601311871" />
<img width="1919" height="960" alt="image" src="https://github.com/user-attachments/assets/c16e61bc-d94e-426e-a2b5-dca887beec68" />


Mostrar imagen
Aplicación nativa multiplataforma desarrollada con .NET MAUI para Android y Windows que ofrece una plataforma de minijuegos con ranking global en tiempo real, sistema de logros y funcionamiento híbrido online/offline.

🚀 Características Principales

4 minijuegos: Topos, Wordle, Parejas y 2048
Ranking en tiempo real con SignalR entre todos los dispositivos
Funcionamiento offline con sincronización automática al recuperar internet
Sistema de logros y XP con subida de nivel automática
Verificación de email en el registro con SendGrid
Seguridad: contraseñas cifradas con SHA256, API REST como intermediario


🏗️ Arquitectura

<img width="877" height="595" alt="image" src="https://github.com/user-attachments/assets/5f2c9ec0-a1c2-4bad-af21-fdda165938bd" />


🛠️ Tecnologías
CapaTecnologíaFrontend.NET MAUI BackendASP.NET Core Web APIBD localSQLite (sqlite-net-pcl)BD en la nubeMySQL en AivenTiempo realSignalRHosting APIRailwayEmailSendGridControl de versionesGitHub

📂 Estructura del Proyecto
FrivGame_Minijuegos_FAFA_APP/
├── API/                    ──► API REST con ASP.NET Core y SignalR
├── BGestionFAFA/           ──► Clases de acceso a datos (SQLite y REST)
├── BModelosFAFA/           ──► Modelos de negocio
├── BModelosSQLFAFA/        ──► Modelos SQLite
├── BViewsFAFA/             ──► Componentes reutilizables (ContentViews)
└── FrivGame_Minijuegos_FAFA_APP/  ──► Proyecto MAUI principal

⚙️ Aspectos Técnicos Destacados
Sistema híbrido online/offline
Las partidas jugadas sin conexión se guardan con IDs negativos temporales en SQLite. Al recuperar internet se sincronizan con la nube y obtienen su ID real.
Ranking en tiempo real
Cuando un jugador supera su récord personal la API notifica a todos los dispositivos conectados mediante SignalR, actualizando el ranking automáticamente.
Seguridad
Las credenciales de la base de datos nunca están expuestas en la app cliente. Toda la comunicación pasa por la API REST desplegada en Railway donde las credenciales se almacenan como variables de entorno.

👤 Autor
Francisco Alberto — Proyecto Final de DAM 2025/2026
