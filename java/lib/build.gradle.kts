/*
 * This file was generated by the Gradle 'init' task.
 *
 * This generated file contains a sample Java library project to get you started.
 * For more details take a look at the 'Building Java & JVM projects' chapter in the Gradle
 * User Manual available at https://docs.gradle.org/7.2/userguide/building_java_projects.html
 */

plugins {
    `application`
    id("com.diffplug.spotless") version "5.15.1"
}

repositories {
    // Use Maven Central for resolving dependencies.
    mavenCentral()

    maven("https://momento.jfrog.io/artifactory/maven-public")
}

dependencies {
    implementation("momento.sandbox:momento-sdk:0.18.0")

    // Use JUnit Jupiter for testing.
    testImplementation("org.junit.jupiter:junit-jupiter:5.7.2")
}

spotless {
    java {
        removeUnusedImports()
        googleJavaFormat("1.11.0")
    }
}

tasks.test {
    // Use JUnit Platform for unit tests.
    useJUnitPlatform()
}

application {
    mainClass.set("momento.client.example.MomentoCacheApplication")
}
