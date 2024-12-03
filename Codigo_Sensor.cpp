```C
#include <SPI.h>
#include <LoRa.h>
#include <esp_sleep.h>

// Pines para el m�dulo LoRa
#define NSS 5     // Pin de selecci�n de esclavo (NSS o CS)
#define RST 14    // Pin de reinicio (Reset)
#define DIO0 26   // Pin de interrupci�n (DIO0)

// Pines para el sensor ultras�nico
#define TRIG_PIN 4
#define ECHO_PIN 16

// Alturas del tanque (en cm)
const float ALTURA_MAXIMA = 300.0; // 3 metros = 0% de llenado
const float ALTURA_MINIMA = 20.0;  // 20 cm = 100% de llenado

// Configuraci�n de tiempos (en milisegundos y microsegundos)
const unsigned long activeDuration = 60000; // 1 minuto de mediciones
const unsigned long sleepDuration = 15 * 60 * 1000000; // 15 minutos en microsegundos

void setup() {
  Serial.begin(115200);
  while (!Serial);

  // Configurar los pines del sensor ultras�nico
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);

  Serial.println("Iniciando LoRa...");

  // Configurar los pines del m�dulo LoRa
  LoRa.setPins(NSS, RST, DIO0);

  // Iniciar el m�dulo LoRa en la frecuencia adecuada (433E6, 868E6 o 915E6)
  if (!LoRa.begin(433E6)) {  // Cambia la frecuencia seg�n el m�dulo
    Serial.println("Error al iniciar LoRa");
    while (1);
  }

  Serial.println("LoRa y sensor ultras�nico listos");
}

void loop() {
  unsigned long startTime = millis();

  // Realizar mediciones y recepci�n durante 1 minuto
  while (millis() - startTime < activeDuration) {
    // Medir la distancia con el sensor ultras�nico
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
    } else {
      Serial.println("Distancia es 0 cm, no se env�a mensaje");
    }

    // Recepci�n de mensajes LoRa
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
    }

    delay(5000); // Espera 5 segundos antes de la pr�xima medici�n
  }

  // Entrar en modo sleep por 15 minutos
  Serial.println("Entrando en modo sleep...");
  delay(100); // Peque�a espera antes de entrar en sleep
  LoRa.end(); // Desactivar LoRa para ahorrar energ�a
  esp_sleep_enable_timer_wakeup(sleepDuration);
  esp_deep_sleep_start();

  // El c�digo continuar� desde setup() despu�s de despertar
}

// Funci�n para medir la distancia con el sensor ultras�nico
float medirDistancia() {
  digitalWrite(TRIG_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG_PIN, LOW);

  long duration = pulseIn(ECHO_PIN, HIGH, 30000); // Tiempo m�ximo de espera: 30 ms
  float distance = duration * 0.034 / 2;         // Distancia en cm
  return distance;
}

// Funci�n para calcular el porcentaje de llenado del tanque
float calcularPorcentajeLlenado(float distancia) {
  if (distancia >= ALTURA_MAXIMA) {
    return 0.0; // El tanque est� vac�o
  } else if (distancia <= ALTURA_MINIMA) {
    return 100.0; // El tanque est� lleno
  }

  return ((ALTURA_MAXIMA - distancia) / (ALTURA_MAXIMA - ALTURA_MINIMA)) * 100.0;
}
```