import numpy as np
from tensorflow import keras
import pandas as pd
import pickle

class AnomalyDetector:
    def __init__(self):
        self.model = None
        self.scaler = None
        self.load_model()

    def load_model(self):
        """Load the trained model and scaler"""
        try:
            self.model = keras.models.load_model('AI/trained_autoencoder.keras')
            with open('AI/scaler.pkl', 'rb') as f:
                self.scaler = pickle.load(f)
        except Exception as e:
            raise Exception(f"Error loading model or scaler: {str(e)}")

    def readCsv(self): 
        """Read training data CSV"""
        df = pd.read_csv('AI/data.csv', sep=';', header=None)
        return df[0].values, df.iloc[:, 1:].values

    def predict(self, input_data):
        """
        Make predictions and return reconstruction error and reconstructed values
        
        Args:
            input_data: numpy array of shape (n_samples, n_features) or single sample (n_features,)
        
        Returns:
            dict containing reconstruction error and reconstructed values
        """
        # Ensure input is 2D
        if input_data.ndim == 1:
            input_data = input_data.reshape(1, -1)

        # Normalize the input
        normalized_data = self.scaler.transform(input_data)
        
        # Get reconstructed data
        reconstructed_normalized = self.model.predict(normalized_data)
        
        # Denormalize the reconstructed data
        reconstructed_data = self.scaler.inverse_transform(reconstructed_normalized)
        
        # Calculate reconstruction error
        mse = np.mean(np.power(input_data - reconstructed_data, 2), axis=1)
        
        return {
            "reconstruction_error": mse.tolist(),
            "reconstructed_values": reconstructed_data.tolist()
        }

    def get_threshold(self, percentile=95):
        """
        Calculate the anomaly threshold based on training data
        
        Args:
            percentile: percentile to use for threshold (default: 95)
        
        Returns:
            float: threshold value
        """
        # Load training data
        _, values = self.readCsv()
        
        # Get reconstruction errors for all training data
        normalized_data = self.scaler.transform(values)
        reconstructed_normalized = self.model.predict(normalized_data)
        reconstructed_data = self.scaler.inverse_transform(reconstructed_normalized)
        mse = np.mean(np.power(values - reconstructed_data, 2), axis=1)
        
        # Calculate threshold
        threshold = np.percentile(mse, percentile)
        return float(threshold) 