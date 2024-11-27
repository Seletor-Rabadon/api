import numpy as np
from tensorflow import keras
import pandas as pd
import pickle
import json
import os

os.environ['PYTHONIOENCODING'] = 'utf-8'

class ChampionRecommender:
    def __init__(self):
        # Load the trained model
        self.model = keras.models.load_model('trained_autoencoder.keras')
        
        # Load the scaler
        with open('scaler.pkl', 'rb') as f:
            self.scaler = pickle.load(f)
    
    def predict(self):
        # Read the prediction data
        df = pd.read_csv('prediction_data.csv', sep=';', header=None)
        puuid = df[0].values[0]  # Get the puuid
        values = df.iloc[:, 1:].values  # Get the champion levels
        
        # Normalize the input data
        values_normalized = self.scaler.transform(values)
        
        # Get the prediction
        reconstructed_normalized = self.model.predict(values_normalized, verbose=0)
        
        # Denormalize the prediction
        reconstructed = self.scaler.inverse_transform(reconstructed_normalized)
        
        # Calculate reconstruction error
        mse = np.mean(np.power(values - reconstructed, 2), axis=1)
        
        # Create results dictionary
        results = {
            "puuid": puuid,
            "reconstruction_error": float(mse[0]),
            "original_values": values[0].tolist(),
            "reconstructed_values": reconstructed[0].tolist()
        }
        
        # Save results to JSON file
        with open('prediction_result.json', 'w') as f:
            json.dump(results, f, indent=4)

if __name__ == "__main__":
    recommender = ChampionRecommender()
    recommender.predict()