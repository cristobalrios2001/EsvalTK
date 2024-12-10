#include <SPI.h>
#include <LoRa.h>
#include "esp_bt.h"
#include <WiFi.h>

// Pines para el módulo LoRa
#define NSS 5     // Pin de selección de esclavo (NSS o CS)
#define RST 14    // Pin de reinicio (Reset)
#define DIO0 26   // Pin de interrupción (DIO0)

// Pines para el sensor ultrasónico
#define TRIG_PIN 4
#define ECHO_PIN 16

//--------------------------
// Variables de configuración
const float ALTURA_MAXIMA = 152.0; // Altura máxima del tanque (en cm) = 0% de llenado
const float ALTURA_MINIMA = 37.0;  // Altura mínima del tanque (en cm) = 100% de llenado

const unsigned long MEASUREMENT_TIME = 60000;       // Tiempo total de medición (en ms)
const unsigned long MEASUREMENT_INTERVAL = 15000;   // Intervalo entre mediciones (en ms)
const unsigned long STABILIZATION_DELAY = 5000;     // Tiempo de espera para estabilización (en ms)
const unsigned long SLEEP_TIME = 15 * 60 * 1000000; // Tiempo en modo deep sleep (15 minutos en µs)
//--------------------------

void setup() {
  Serial.begin(115200);
  while (!Serial);

  // Deshabilitar WiFi y Bluetooth para ahorrar energía en ESP32
  WiFi.mode(WIFI_OFF);
  WiFi.disconnect(true);
  btStop(); // Detiene el Bluetooth en el ESP32        // Detiene el Bluetooth en el ESP32

  // Pines del sensor ultrasónico
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);

  // Iniciar LoRa
  Serial.println("Iniciando LoRa...");
  LoRa.setPins(NSS, RST, DIO0);
  if (!LoRa.begin(433E6)) {
    Serial.println("Error al iniciar LoRa");
    while (1);
  }
  Serial.println("LoRa y sensor ultrasónico listos");
}


void loop() {
  unsigned long startTime = millis();
  int validReadings = 0;         // Contador de lecturas válidas
  float totalPercentage = 0.0;   // Acumulador de porcentajes

  // Arreglo para almacenar las lecturas individuales
  // Suponiendo un máximo de 10 lecturas (puede ajustarse si se modifica el tiempo/intervalo)
  float readings[10];

  // Tomar mediciones durante el tiempo configurado
  while (millis() - startTime < MEASUREMENT_TIME) {
    unsigned long intervalStart = millis();

    // Ciclos de estabilización antes de cada medición
    for (int i = 0; i < 3; i++) {
      Serial.print("Estabilización (espera de ");
      Serial.print(STABILIZATION_DELAY / 1000);
      Serial.println(" segundos)");
      delay(STABILIZATION_DELAY);
    }

    // Realizar la medición
    float distance = medirDistancia();
    if (distance > 0) {
      float percentage = calcularPorcentajeLlenado(distance);
      totalPercentage += percentage;
      readings[validReadings] = percentage;
      validReadings++;

      // Mostrar la lectura en el monitor serie
      Serial.print("Distancia medida: ");
      Serial.print(distance);
      Serial.println(" cm");
      Serial.print("Nivel calculado: ");
      Serial.print(percentage);
      Serial.println("%");
    } else {
      Serial.println("Lectura inválida, ignorada.");
    }

    // Esperar a completar el intervalo de medición
    while (millis() - intervalStart < MEASUREMENT_INTERVAL) {
      delay(100);
    }
  }

  // Calcular el promedio si hay lecturas válidas
  if (validReadings > 0) {
    float averagePercentage = totalPercentage / validReadings;

    // Enviar las lecturas individuales por LoRa
    LoRa.beginPacket();
    LoRa.print("Lecturas individuales: ");
    for (int i = 0; i < validReadings; i++) {
      LoRa.print(readings[i]);
      if (i < validReadings - 1) {
        LoRa.print(", ");
      }
    }
    LoRa.endPacket();

    delay(500); // Breve retraso entre paquetes

    // Enviar el promedio por LoRa
    LoRa.beginPacket();
    LoRa.print("Nivel promedio del tanque: ");
    LoRa.print(averagePercentage);
    LoRa.println("%");
    LoRa.endPacket();

    Serial.print("Nivel promedio enviado: ");
    Serial.print(averagePercentage);
    Serial.println("%");
  } else {
    Serial.println("No se tomaron lecturas válidas en este ciclo.");
  }

  // Entrar en modo deep sleep
  Serial.println("Entrando en modo sleep...");
  Serial.flush(); // Asegurarse de que se envíen todos los datos por Serial
  delay(100);     // Pequeña espera antes de entrar en sleep
  LoRa.end();     // Desactivar LoRa para ahorrar energía

  // Configurar el temporizador para despertar después del tiempo configurado
  esp_sleep_enable_timer_wakeup(SLEEP_TIME);
  esp_deep_sleep_start();
}

// Función para medir la distancia con el sensor ultrasónico
float medirDistancia() {
  digitalWrite(TRIG_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG_PIN, LOW);

  long duration = pulseIn(ECHO_PIN, HIGH, 30000); // Tiempo máximo de espera: 30 ms

  // Verificar si se obtuvo una lectura válida
  if (duration == 0) {
    return -1.0; // Timeout ocurrido, lectura inválida
  }

  float distance = duration * 0.034 / 2.0; // Distancia en cm
  return distance;
}

// Función para calcular el porcentaje de llenado del tanque
float calcularPorcentajeLlenado(float distancia) {
  if (distancia >= ALTURA_MAXIMA) {
    return 0.0; // El tanque está vacío
  } else if (distancia <= ALTURA_MINIMA) {
    return 100.0; // El tanque está lleno
  }
  return ((ALTURA_MAXIMA - distancia) / (ALTURA_MAXIMA - ALTURA_MINIMA))*100.0;
}
