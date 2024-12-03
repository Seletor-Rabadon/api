import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '1'
os.environ['TF_ENABLE_ONEDNN_OPTS'] = '0'

import json
import pandas as pd
import numpy as np
from tensorflow import keras
from sklearn.preprocessing import MinMaxScaler
from sklearn.model_selection import train_test_split
import pickle

def readCsv(): 
    # Read CSV with semicolon separator and no header
    df = pd.read_csv('data.csv', sep=';', header=None)
    
    # Separate puuids and values
    puuids = df[0].values
    values = df.iloc[:, 1:].values
    
    return puuids, values

def create_autoencoder(input_dim):
    # Input layer
    input_layer = keras.layers.Input(shape=(input_dim,))
    
    encoded = keras.layers.Dense(24, activation='relu', 
                               kernel_regularizer=keras.regularizers.l2(0.01))(input_layer)
    decoded = keras.layers.Dense(input_dim, activation='linear')(encoded)
    
    # Create model
    autoencoder = keras.Model(input_layer, decoded)
    
    return autoencoder

def normalize_data(values):
    """
    Normalize the input data using MinMaxScaler and save the scaler
    
    Args:
        values: numpy array of values to normalize
        
    Returns:
        normalized_values: The normalized data
        scaler: The fitted MinMaxScaler object
    """
    # Clip values to desired ranges  25
    values = np.clip(values, 0, 20)
    values[values < 2] = 0
    
    scaler = MinMaxScaler()
    values_normalized = scaler.fit_transform(values)
    
    # Save the scaler
    with open('scaler.pkl', 'wb') as f:
        pickle.dump(scaler, f)
        
    return values_normalized, scaler

def add_noise(data, noise_factor=0.05):
    """
    Add random Gaussian noise to the data
    
    Args:
        data: Input data to add noise to
        noise_factor: Amount of noise to add (default: 0.05)
    
    Returns:
        noisy_data: Data with added noise, clipped to [0,1]
    """
    noise = np.random.normal(loc=0.0, scale=noise_factor, size=data.shape)
    noisy_data = data + noise
    # Clip the noisy data to keep values between 0 and 1 (since data is normalized)
    return np.clip(noisy_data, 0, 1)

def train():
    # Read and prepare data
    puuids, values = readCsv()
    
    # Normalize the data using the new function
    values_normalized, scaler = normalize_data(values)
    
    # Split the data
    X_train, X_test = train_test_split(values_normalized, test_size=0.25, random_state=42)
    
    # Create and compile the autoencoder
    input_dim = values.shape[1]
    autoencoder = create_autoencoder(input_dim)
    autoencoder.compile(optimizer='adam', loss='mse')
    
    # Add early stopping and reduce learning rate on plateau
    early_stopping = keras.callbacks.EarlyStopping(
        monitor='val_loss',
        patience=10,
        restore_best_weights=True
    )
    
    reduce_lr = keras.callbacks.ReduceLROnPlateau(
        monitor='val_loss',
        factor=0.25,
        patience=5,
        min_lr=1e-6
    )
    
    # Add noise to training data
    X_train_noisy = add_noise(X_train)
    X_test_noisy = add_noise(X_test)
    
    # Modify training to use noisy input but clean targets
    history = autoencoder.fit(
        X_train_noisy, X_train,  # Input noisy data, but try to reconstruct clean data
        epochs=128,
        batch_size=64,
        shuffle=True,
        validation_data=(X_test_noisy, X_test),  # Same for validation data
        callbacks=[early_stopping, reduce_lr]
    )
    
    # Save the model
    autoencoder.save('trained_autoencoder.keras')
    
    # Get the reconstructed data
    reconstructed_data = autoencoder.predict(values_normalized)
    
    # Denormalize the reconstructed data
    reconstructed_data = scaler.inverse_transform(reconstructed_data)
    
    # Calculate reconstruction error, bias, and variance
    mse = np.mean(np.power(values - reconstructed_data, 2), axis=1)
    bias = np.mean(reconstructed_data - values, axis=1)
    variance = np.var(reconstructed_data - values, axis=1)
    
    mean_bias = float(np.mean(bias))
    mean_abs_bias = float(np.mean(np.abs(bias)))
    mean_variance = float(np.mean(variance))
    
    # Calculate baseline performance (using mean predictions)
    mean_predictions = np.full_like(values, np.mean(values, axis=0))
    baseline_mse = np.mean(np.power(values - mean_predictions, 2)) 
    
    # Calculate cross-validation error (using the validation set)
    cv_predictions = autoencoder.predict(X_test)
    cv_predictions_denorm = scaler.inverse_transform(cv_predictions)
    X_test_denorm = scaler.inverse_transform(X_test)
    cv_error = float(np.mean(np.power(X_test_denorm - cv_predictions_denorm, 2)))
    
    # Create results dictionary with added metrics
    results = { 
        "model_summary": {
            "input_dim": int(input_dim),
            "latent_dim": 72,
            "final_loss": float(history.history['loss'][-1]),
            "final_val_loss": float(history.history['val_loss'][-1]), 
            "mean_bias": mean_bias,
            "mean_absolute_bias": mean_abs_bias, 
            "mean_variance": mean_variance,
            "baseline_performance": float(baseline_mse),
            "cross_validation_error": cv_error, 
        }
    }
    
    # Save results to JSON file
    with open('training_result.json', 'w') as f:
        json.dump(results, f, indent=4)
    
    print("Training completed!")
    print(f"Final loss: {results['model_summary']['final_loss']}")
    print(f"Final validation loss: {results['model_summary']['final_val_loss']}")

if __name__ == "__main__":
    train() 