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
    
    # Encoder layers
    encoded = keras.layers.Dense(128, activation='relu')(input_layer)
    encoded = keras.layers.Dense(64, activation='relu')(encoded)
    encoded = keras.layers.Dense(32, activation='relu')(encoded)
    
    # Add dropout to prevent overfitting
    encoded = keras.layers.Dropout(0.2)(encoded)
    
    # Decoder layers 
    decoded = keras.layers.Dense(64, activation='relu')(encoded)
    decoded = keras.layers.Dense(128, activation='relu')(decoded)
    decoded = keras.layers.Dense(input_dim, activation='sigmoid')(decoded)
    
    # Create model
    autoencoder = keras.Model(input_layer, decoded)
    
    return autoencoder

def train():
    # Read and prepare data
    puuids, values = readCsv()
    
    
    scaler = MinMaxScaler()
    values_normalized = scaler.fit_transform(values)
    
    # Save the scaler
    with open('scaler.pkl', 'wb') as f:
        pickle.dump(scaler, f)
    
    # Split the data
    X_train, X_test = train_test_split(values_normalized, test_size=0.2, random_state=42)
    
    # Create and compile the autoencoder
    input_dim = values.shape[1]
    autoencoder = create_autoencoder(input_dim)
    autoencoder.compile(optimizer='adam', loss='mse')
    
    # Train the model
    history = autoencoder.fit(
        X_train, X_train,
        epochs=100,
        batch_size=64,
        shuffle=True,
        validation_data=(X_test, X_test)
    )
    
    # Save the model
    autoencoder.save('trained_autoencoder.keras')
    
    # Get the reconstructed data
    reconstructed_data = autoencoder.predict(values_normalized)
    
    # Denormalize the reconstructed data
    reconstructed_data = scaler.inverse_transform(reconstructed_data)
    
    # Calculate reconstruction error
    mse = np.mean(np.power(values - reconstructed_data, 2), axis=1)
    
    # Create results dictionary
    results = { 
        "model_summary": {
            "input_dim": int(input_dim),
            "latent_dim": 16,
            "final_loss": float(history.history['loss'][-1]),
            "final_val_loss": float(history.history['val_loss'][-1])
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