#include <SPI.h>
#include <LoRa.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <esp_sleep.h>

//--------------------------------------------------------------

// Alturas del tanque (en cm)
const float ALTURA_MAXIMA = 300.0; // 3 metros = 0% de llenado
const float ALTURA_MINIMA = 20.0;  // 20 cm = 100% de llenado

const int ID_dispositivo = 1;

// Credenciales de WiFi
const char* ssid = "Iphone de Cristóbal";
const char* password = "gilmaximo";

// URL de la API
const char* apiUrl = "https://7570-186-11-2-49.ngrok-free.app/api/Mediciones/RegisterWaterLevelMeasurement";

// Configuración de tiempos (en milisegundos y microsegundos)
const unsigned long activeDuration = 60000; // 1 minuto de mediciones
const unsigned long sleepDuration = 15 * 60 * 1000000; // 15 minutos en microsegundos

//--------------------------------------------------------------

// Pines para el módulo LoRa
#define NSS 5     // Pin de selección de esclavo (NSS o CS)
#define RST 14    // Pin de reinicio (Reset)
#define DIO0 26   // Pin de interrupción (DIO0)

// Pines para el sensor ultrasónico
#define TRIG_PIN 4
#define ECHO_PIN 16


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

  // Configurar los pines del sensor ultrasónico
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);

  Serial.println("Iniciando LoRa...");

  // Configurar los pines del módulo LoRa
  LoRa.setPins(NSS, RST, DIO0);

  // Iniciar el módulo LoRa en la frecuencia adecuada
  if (!LoRa.begin(433E6)) { // Cambia la frecuencia según el módulo
    Serial.println("Error al iniciar LoRa");
    while (1);
  }

  Serial.println("LoRa y sensor ultrasónico listos");
}

void loop() {
  unsigned long startTime = millis();

  // Realizar mediciones y recepción durante 1 minuto
  while (millis() - startTime < activeDuration) {
    // Medir la distancia con el sensor ultrasónico
    float distance = medirDistancia();

    if (distance > 0) {
      // Calcular el porcentaje de llenado
      float porcentaje = calcularPorcentajeLlenado(distance);

      // Imprimir los resultados
      Serial.print("Distancia medida: ");
      Serial.print(distance);
      Serial.println(" cm");

      Serial.print("Nivel del tanque: ");
      Serial.print(porcentaje);
      Serial.println("%");

      // Enviar el porcentaje de llenado por LoRa
      LoRa.beginPacket();
      LoRa.print("Nivel del tanque: ");
      LoRa.print(porcentaje);
      LoRa.println("%");
      LoRa.endPacket();

      Serial.println("Mensaje enviado por LoRa");

      // Enviar los datos a la API
      enviarDatosAPI(porcentaje);
    } else {
      Serial.println("Distancia es 0 cm, no se envía mensaje");
    }

    delay(5000); // Espera 5 segundos antes de la próxima medición
  }

  // Entrar en modo sleep por 15 minutos
  Serial.println("Entrando en modo sleep...");
  delay(100); // Pequeña espera antes de entrar en sleep
  LoRa.end(); // Desactivar LoRa para ahorrar energía
  esp_sleep_enable_timer_wakeup(sleepDuration);
  esp_deep_sleep_start();

  // El código continuará desde setup() después de despertar
}

// Función para medir la distancia con el sensor ultrasónico
float medirDistancia() {
  digitalWrite(TRIG_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG_PIN, LOW);

  long duration = pulseIn(ECHO_PIN, HIGH, 30000); // Tiempo máximo de espera: 30 ms
  float distance = duration * 0.034 / 2;         // Distancia en cm
  return distance;
}

// Función para calcular el porcentaje de llenado del tanque
float calcularPorcentajeLlenado(float distancia) {
  if (distancia >= ALTURA_MAXIMA) {
    return 0.0; // El tanque está vacío
  } else if (distancia <= ALTURA_MINIMA) {
    return 100.0; // El tanque está lleno
  }

  return ((ALTURA_MAXIMA - distancia) / (ALTURA_MAXIMA - ALTURA_MINIMA)) * 100.0;
}

// Función para enviar los datos a la API
void enviarDatosAPI(float nivelAgua) {
  if (WiFi.status() == WL_CONNECTED) {
    HTTPClient http;
    http.begin(apiUrl);
    http.addHeader("Content-Type", "application/json");

    // Crear el payload en formato JSON
    String payload = "{\"IdDispositivo\":" + String(ID_dispositivo) + ",\"NivelAgua\":" + String(nivelAgua) + "}";

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
