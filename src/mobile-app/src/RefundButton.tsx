import React, { useState } from 'react';
import { StyleSheet, Text, TouchableOpacity, View, ActivityIndicator } from 'react-native';

interface RefundButtonProps {
  chargeId: string;
  amount: number;
}

export const RefundButton: React.FC<RefundButtonProps> = ({ chargeId, amount }) => {
  const [loading, setLoading] = useState<boolean>(false);
  const [status, setStatus] = useState<string>('');

  const executeRefund = async () => {
    setLoading(true);
    setStatus('');

    // OBSERVABILITY CRITICAL STEP: Generate an end-to-end tracing correlation ID
    const correlationId = `CORR-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

    try {
      const response = await fetch('https://api.company.com/payments/refund', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-Correlation-ID': correlationId
        },
        body: JSON.stringify({
          ChargeId: chargeId,
          Amount: amount
        })
      });

      if (!response.ok) {
        throw new Error(`Server returned status code: ${response.status}`);
      }

      setStatus('Refund Successfully Initiated');
    } catch (error: any) {
      setStatus(`Refund Failed: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={styles.container}>
      <TouchableOpacity style={styles.button} onPress={executeRefund} disabled={loading}>
        {loading ? <ActivityIndicator color="#FFFFFF" /> : <Text style={styles.text}>Process Refund</Text>}
      </TouchableOpacity>
      {status ? <Text style={status.includes('Failed') ? styles.errorText : styles.statusText}>{status}</Text> : null}
    </View>
  );
};

const styles = StyleSheet.create({
  container: { padding: 16 },
  button: { backgroundColor: '#D9534F', padding: 12, borderRadius: 8, alignItems: 'center' },
  text: { color: '#FFFFFF', fontWeight: 'bold', fontSize: 16 },
  statusText: { marginTop: 8, textAlign: 'center', color: '#333333' },
  errorText: { marginTop: 8, textAlign: 'center', color: '#D9534F' }
});
