#include <SPI.h>
#include <LoRa.h>
#include <WiFi.h>
#include <HTTPClient.h>

// Pines para el módulo LoRa
#define NSS 5     // Pin de selección de esclavo (NSS o CS)
#define RST 14    // Pin de reinicio (Reset)
#define DIO0 26   // Pin de interrupción (DIO0)

// Credenciales de WiFi
const char* ssid = "Iphone de Cristóbal";
const char* password = "gilmaximo";

// URL de la API
const char* apiUrl = "https://7570-186-11-2-49.ngrok-free.app/api/Mediciones/RegisterWaterLevelMeasurement";

// ID del dispositivo receptor
const char* deviceId = "209290626";

void setup() {
  Serial.begin(115200);
  while (!Serial);

  // Conexión a WiFi
  Serial.println("Conectando a WiFi...");
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Conectando...");
  }
  Serial.println("Conexión WiFi establecida");

  // Inicializar LoRa
  Serial.println("Iniciando LoRa receptor...");
  LoRa.setPins(NSS, RST, DIO0);
  if (!LoRa.begin(433E6)) {
    Serial.println("Error al iniciar LoRa receptor");
    while (1);
  }
  Serial.println("LoRa receptor listo para recibir mensajes");
}

void loop() {
  // Verificar si se ha recibido un paquete
  int packetSize = LoRa.parsePacket();
  if (packetSize) {
    Serial.print("Mensaje recibido: ");

    // Leer el paquete recibido
    String message = "";
    while (LoRa.available()) {
      message += (char)LoRa.read();
    }

    // Mostrar el mensaje recibido
    Serial.println(message);

    // Extraer el nivel de agua del mensaje
    float nivelAgua = extraerNivelAgua(message);

    if (nivelAgua >= 0.0) {
      Serial.print("Nivel de agua extraído: ");
      Serial.println(nivelAgua);

      // Enviar a la API
      enviarDatosAPI(nivelAgua);
    } else {
      Serial.println("Error al extraer el nivel de agua del mensaje");
    }
  }
}

// Función para extraer el nivel de agua del mensaje recibido
float extraerNivelAgua(const String& mensaje) {
  // Asumimos que el mensaje tiene el formato "Nivel del tanque: X%"
  int inicio = mensaje.indexOf(":");
  int fin = mensaje.indexOf("%");

  if (inicio > 0 && fin > inicio) {
    String nivelStr = mensaje.substring(inicio + 2, fin);
    return nivelStr.toFloat();  // Convertir a flotante
  }
  return -1; // Error al extraer
}

// Función para enviar los datos a la API
void enviarDatosAPI(float nivelAgua) {
  if (WiFi.status() == WL_CONNECTED) {
    HTTPClient http;
    http.begin(apiUrl);
    http.addHeader("Content-Type", "application/json");

    // Crear el payload en formato JSON
    String payload = "{\"IdDispositivo\":\"" + String(deviceId) + "\",\"NivelAgua\":" + String(nivelAgua) + "}";

    // Enviar la solicitud POST
    int httpResponseCode = http.POST(payload);

    if (httpResponseCode > 0) {
      Serial.print("Respuesta de la API: ");
      Serial.println(httpResponseCode);
      Serial.println(http.getString());
    } else {
      Serial.print("Error al enviar datos: ");
      Serial.println(http.errorToString(httpResponseCode).c_str());
    }
    http.end();
  } else {
    Serial.println("WiFi no conectado, no se pueden enviar datos");
  }
}
