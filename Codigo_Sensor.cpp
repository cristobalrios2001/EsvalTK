#include <SPI.h>
#include <LoRa.h>

// Pines para el módulo LoRa
#define NSS 5     // Pin de selección de esclavo (NSS o CS)
#define RST 14    // Pin de reinicio (Reset)
#define DIO0 26   // Pin de interrupción (DIO0)

// Pines para el sensor ultrasónico
#define TRIG_PIN 4
#define ECHO_PIN 16

// Alturas del tanque (en cm)
const float ALTURA_MAXIMA = 300.0; // 3 metros = 0% de llenado
const float ALTURA_MINIMA = 20.0;  // 20 cm = 100% de llenado

// Configuración de tiempos
const unsigned long MEASUREMENT_TIME = 60000; // Tiempo de medición: 1 minuto (en ms)
const unsigned long MEASUREMENT_INTERVAL = 15000; // Intervalo entre mediciones: 15 segundos (en ms)
const unsigned long STABILIZATION_DELAY = 5000; // Espera entre estabilizaciones: 5 segundos (en ms)
void setup() {
  Serial.begin(115200);
  while (!Serial);
  // Configurar los pines del sensor ultrasónico
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);
  Serial.println("Iniciando LoRa...");
  // Configurar los pines del módulo LoRa
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
  // Tomar mediciones durante 1 minuto
  while (millis() - startTime < MEASUREMENT_TIME) {
    for (int i = 0; i < 4; i++) { // Cada ciclo de 15 segundos toma una medición
      unsigned long intervalStart = millis();
      // Realizar la medición al final de cada ciclo de 15 segundos
      if (i == 2) {
        float distance = medirDistancia();
        if (distance > 0) {
          float percentage = calcularPorcentajeLlenado(distance);
          totalPercentage += percentage; // Acumular porcentaje
          validReadings++;               // Contar lectura válida
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
      } else {
        Serial.println("Espera"); 
      }
      // Asegurarse de esperar 5 segundos en cada ciclo
      while (millies() - currentTime)-TIME-LINE  
