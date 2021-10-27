## Running the Example

- JDK 11 or above is required to run the example
- You do not need gradle to be installed


```bash
cd java/
MOMENTO_AUTH_TOKEN=<YOUR AUTH TOKEN> ./gradlew run
```

Example Code: [MomentoApplication.java](lib/src/main/java/momento/client/example/MomentoCacheApplication.java)

- Send us an email at [support@momentohq.com](mailto:support@momentohq.com) to request a Momento Auth Token. It is required to authenticate with the Momento Cache Service. This token uniquely identifies cache interactions. The token should be treated like a sensitive password and all essential care must be taken to ensure its secrecy. We recommend that you store this token in a secret vault like AWS Secrets Manager.

## Using the Java SDK in your project

### Gradle Configuration

Update your Gradle build to include the components

**build.gradle.kts**

```kotlin
repositories {
    maven("https://momento.jfrog.io/artifactory/maven-public")
}

dependencies {
    implementation("momento.sandbox:momento-sdk:0.14.0")
}
```
