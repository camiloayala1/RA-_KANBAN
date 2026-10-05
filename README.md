# AR KANBAN 📌📱
> Sistema interactivo de gestión ágil de tareas en Realidad Aumentada para Android desarrollado con Unity y Firebase.

AR KANBAN permite proyectar, anclar e interactuar con un tablero Kanban completo directamente sobre cualquier pared o superficie plana de tu espacio de trabajo, combinando la ergonomía del mundo físico con la persistencia digital en la nube.

---

## 🚀 Características Principales

* **Detección Automática de Planos:** Reconocimiento de paredes (verticales) y escritorios (horizontales) en tiempo real mediante AR Foundation[cite: 3].
* **Orientación Inteligente:** El tablero calcula automáticamente la normal de la superficie y se orienta mirando hacia el usuario[cite: 3].
* **Anclaje Robusto (Anti-Drift):** Uso de `ARAnchorManager` para mantener el tablero fijo en el entorno real sin desplazamientos no deseados[cite: 3].
* **Interacción Táctil 3D:** Creación dinámica de notas tipo Post-it con texto renderizado mediante TextMeshPro, arrastre entre columnas y eliminación por colisión con papelera[cite: 1].
* **Backend Ligero sin SDKs Pesados:** Conexión con Firebase Authentication y Firebase Realtime Database a través de solicitudes HTTP REST (`UnityWebRequest`).
* **Optimización de Rendimiento:** Desactivación selectiva de nubes de puntos y mallas de planos al fijar el tablero para reducir llamadas de dibujado (*draw calls*)[cite: 3].
* **Interfaz Responsive:** HUD adaptado a dispositivos móviles de alta resolución (base 1080 × 2400).

---

## 🛠️ Stack Tecnológico

| Herramienta | Versión / Tipo | Uso |
| :--- | :--- | :--- |
| **Unity** | 2023.1+ (LTS) | Motor gráfico y lógica general |
| **AR Foundation** | 5.x+ | Abstracción de subsistemas de Realidad Aumentada[cite: 3] |
| **ARCore XR Plugin** | Compatible | Tracking espacial, planos y anclajes en Android[cite: 3] |
| **TextMeshPro** | Nativo | Renderizado tipográfico 2D y etiquetas en tarjetas 3D[cite: 1, 3] |
| **Firebase REST API** | Auth & Realtime DB | Autenticación de usuarios y persistencia de notas |
| **C#** | .NET Standard 2.1 | Desarrollo de scripts y algoritmos |

---

## 📂 Arquitectura de Scripts Principales

* `BoardPlacementManager.cs`: Administra el raycasting espacial, detección de planos estables, anclaje del tablero e intercambio de estados en la interfaz HUD[cite: 3].
* `KanbanBoardManager.cs`: Gestiona la instanciación de tarjetas, asignación a columnas (*Por Hacer*, *En Proceso*, *Hecho*) y sincronización de datos[cite: 1, 3].
* `CardInteraction.cs`: Controla el arrastre táctil (*drag and drop*) de las notas sobre el plano y las colisiones con el objeto papelera[cite: 1].
* `FirebaseAuthREST.cs`: Maneja el registro e inicio de sesión seguro contra Google Identity Toolkit y persistencia local de token en `PlayerPrefs`.
* `FirebaseManager.cs`: Serializa y deserializa datos JSON de los tableros en Firebase Realtime Database.

---

## 📲 Requisitos e Instalación

### Requisitos del Dispositivo
* Dispositivo móvil con Android 8.0 (API Level 26) o superior.
* Soporte nativo para **Google Play Services for AR (ARCore)** (probado en Xiaomi Redmi Note 11).

### Compilación desde el Editor
1. Clonar el repositorio:
   ```bash
   git clone [https://github.com/tu-usuario/AR-Kanban.git](https://github.com/tu-usuario/AR-Kanban.git)
