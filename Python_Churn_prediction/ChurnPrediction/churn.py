import pandas as pd
import numpy as np
import mysql.connector
from sqlalchemy import create_engine
from sklearn.model_selection import LeaveOneOut
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import classification_report, confusion_matrix
import matplotlib.pyplot as plt
import seaborn as sns
import time, os

def create_db_engine():
    user = 'root'
    password = 'cryptocart@01'
    host = 'localhost'
    database = 'ecommerce'
    return mysql.connector.connect(host=host, user=user, password=password, database=database)

def load_data(engine):
    query_users = "SELECT Id, EmailConfirmed, PhoneNumberConfirmed, HasChurned FROM aspnetusers;"
    query_orders = "SELECT UserId, COUNT(*) AS OrderCount, AVG(OrderTotal) AS AvgOrderValue FROM orders GROUP BY UserId;"
    users = pd.read_sql(query_users, engine)
    orders = pd.read_sql(query_orders, engine)
    data = pd.merge(users, orders, left_on='Id', right_on='UserId', how='left')
    data.fillna({'OrderCount': 0, 'AvgOrderValue': 0}, inplace=True)
    return data

def prepare_features_labels(data):
    features = data[['EmailConfirmed', 'PhoneNumberConfirmed', 'OrderCount', 'AvgOrderValue']]
    labels = data['HasChurned']
    return features, labels

def visualize_results(y_true, y_pred, classifier, features):
    cm = confusion_matrix(y_true, y_pred)
    sns.heatmap(cm, annot=True, fmt='d', cmap='Blues')
    plt.title('Confusion Matrix')
    plt.xlabel('Predicted Labels')
    plt.ylabel('True Labels')
    if os.path.exists('confusionMatrix.png'):
        os.remove('confusionMatrix.png')
    plt.savefig('confusionMatrix.png')
    plt.show()
    time.sleep(1)
    importances = classifier.feature_importances_
    indices = np.argsort(importances)
    plt.title('Feature Importances')
    plt.barh(range(len(indices)), importances[indices], color='b', align='center')
    plt.yticks(range(len(indices)), [features.columns[i] for i in indices])
    plt.xlabel('Relative Importance')
    if os.path.exists('FeatureImportance.png'):
        os.remove('FeatureImportance.png')
    plt.savefig('FeatureImportance.png')
    plt.show()

def main():
    engine = create_db_engine()
    data = load_data(engine)
    features, labels = prepare_features_labels(data)
    loo = LeaveOneOut()
    classifier = RandomForestClassifier(random_state=42)
    predictions = []
    actuals = []
    user_ids = data['Id'].tolist()
    for train_index, test_index in loo.split(features):
        X_train, X_test = features.iloc[train_index], features.iloc[test_index]
        y_train, y_test = labels.iloc[train_index], labels.iloc[test_index]
        classifier.fit(X_train, y_train)
        pred = classifier.predict(X_test)
        predictions.extend(pred)
        actuals.extend(y_test)
    print(classification_report(actuals, predictions))
    visualize_results(actuals, predictions, classifier, features)
    visualize_user_predictions(user_ids, predictions)
    for user_id, prediction in zip(user_ids, predictions):
        print(f"User ID {user_id} is predicted to {'churn' if prediction else 'not churn'}.")

def visualize_user_predictions(user_ids, predictions):
    plt.figure(figsize=(10, len(user_ids)))
    plt.barh(user_ids, predictions, color='skyblue')
    plt.xlabel('Churn Prediction (1 = Churn, 0 = Not Churn)')
    plt.ylabel('User ID')
    plt.title('Churn Predictions for Users')
    if os.path.exists('UserChurnPrediction.png'):
        os.remove('UserChurnPrediction.png')
    plt.savefig('UserChurnPrediction.png')
    plt.show()

if __name__ == '__main__':
    main()
