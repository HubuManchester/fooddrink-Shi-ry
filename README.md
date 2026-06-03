# Food Explorer - .NET MAUI Food Tracking App

This is a cross-platform mobile application built with .NET MAUI for recording and managing food and drink items.

**Framework:** .NET MAUI 8.0  
**Platforms:** Android phone, Android pad, Windows, 


## Features

### Food Management

- Display food items in a card-style list view
- Real-time search by name, category, or description
- Add new food items with form validation
- Edit existing food information
- Delete food items with confirmation dialog
- View detailed nutrition information including calories, protein, carbs, and fat

### Hardware Integration

- **Camera** - Take and save photos of food items
- **Text-to-Speech** - Read food descriptions and nutrition information aloud
- **Vibration** - Provide tactile feedback on button clicks
- **Haptic Feedback** - Touch response feedback for user actions
- **GPS Location** - Record dining location using device geolocation
- **Maps Integration** - Open saved locations in system maps application
- **Accelerometer and Shake** - Detect device shake to add items to favorites

### Favorites System

- Add or remove food items from favorites
- Shake device to quickly add current item to favorites
- Dedicated favorites page displaying all saved items
- Remove favorites directly from the favorites list

### Accessibility

- Dark mode and light theme switching
- Large text mode for better readability
- Screen reader support using SemanticProperties
- User theme preference saved automatically

### Data Persistence

- SQLite local database for offline data storage
- Photos saved to application local directory
- Location coordinates and address information saved
- Created timestamp for each food item


## Hardware Features Used

- Camera
- Text-to-Speech
- Vibration
- Haptic Feedback
- GPS / Geolocation
- Maps
- Accelerometer (Shake Detection)


## Project Architecture

The application follows the MVVM (Model-View-ViewModel) pattern with dependency injection. Views are defined in XAML, ViewModels handle business logic, and Services manage data operations including database access, speech synthesis, and accessibility features.


## How to Run

Open the solution in Visual Studio 2022, select the target platform (Android or Windows), and press F5 to run.

**Build commands:**

For Android:
dotnet build -f net8.0-android

For Windows:
dotnet build -f net8.0-windows10.0.19041.0


## Permissions Required

The app requests the following permissions at runtime:
- Camera access for taking food photos
- Location access for recording dining locations
- Vibration for haptic feedback


