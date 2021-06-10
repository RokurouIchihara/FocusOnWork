package com.example.FocusOnWork;

/**
 * Example local unit test, which will execute on the development machine (host).
 *
 * @see <a href="http://d.android.com/tools/testing">Testing documentation</a>
 */
public class ExampleUnitTest {
    public static final String TAG = "SampleActivity";

    private static final int REQUEST_ENABLE_BT = 2;
    private static final int MESSAGE_CLIENT_ACCEPT = 1;
    private static final int MESSAGE_RECEIVE = 2;

    private static final UUID MY_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");

    private BluetoothAdapter adapter;
    private BluetoothSocket socket;
    private ArrayAdapter<String> arrays;
    private AcceptThread mAcceptThread;
    private ConnectThread mConnectThread;

    private final Handler handler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case MESSAGE_CLIENT_ACCEPT:
                    if (msg.obj != null && msg.obj instanceof BluetoothSocket) {
                        if (mConnectThread != null) {
                            mConnectThread.cancel();
                            mConnectThread = null;
                        }

                        LinearLayout layout = (LinearLayout)findViewById(R.id.layout);

                        if (layout.getVisibility() == View.INVISIBLE) {
                            layout.setVisibility(View.VISIBLE);
                        }

                        BluetoothSocket socket = (BluetoothSocket)msg.obj;

                        mConnectThread = new ConnectThread(socket);
                        mConnectThread.start();
                    }

                    break;

                case MESSAGE_RECEIVE:
                    if (msg.obj != null) {
                        String text = (String)msg.obj;

                        if (!TextUtils.isEmpty(text)) {
                            arrays.add(text);
                        }
                    }

                    break;
            }
        }
    };

    @Override
    public void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.main);

        ((Button)findViewById(R.id.btn)).setOnClickListener(new View.OnClickListener() {
            public void onClick(View view) {
                String text = ((EditText)findViewById(R.id.text)).getText().toString();

                if (TextUtils.isEmpty(text)) {
                    return;
                }

                try {
                    if (socket == null) {
                        Set<BluetoothDevice> devices = adapter.getBondedDevices();

                        if (devices.size() <= 0) {
                            return;
                        }

                        BluetoothDevice device = devices.iterator().next();
                        socket = device.createRfcommSocketToServiceRecord(MY_UUID);
                        socket.connect();
                    }

                    socket.getOutputStream().write(text.getBytes());
                } catch (IOException e) {
                    e.printStackTrace();

                    Toast.makeText(
                            SampleActivity.this,
                            String.format("connect unavailable: %s", socket.getRemoteDevice().getName()),
                            Toast.LENGTH_LONG
                    ).show();
                } finally {
                    if (socket != null) {
                        try {
                            socket.close();
                        } catch (IOException e) {
                            e.printStackTrace();
                        }
                    }
                }
            }
        });

        arrays = new ArrayAdapter<String>(this, R.layout.messages);

        ((ListView)findViewById(R.id.listView)).setAdapter(arrays);
    }

    @Override
    public void onStart() {
        super.onStart();

        adapter = BluetoothAdapter.getDefaultAdapter();

        if (adapter == null) {
            finish();

            return;
        }

        if (!adapter.isEnabled()) {
            Intent intent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            startActivityForResult(intent, REQUEST_ENABLE_BT);
        } else {
            onBonding();
        }
    }

    @Override
    public void onStop() {
        super.onStop();

        if (socket != null) {
            try {
                socket.close();
                socket = null;
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

        if (mConnectThread != null) {
            mConnectThread.cancel();
            mConnectThread = null;
        }

        if (mAcceptThread != null) {
            mAcceptThread.cancel();
            mAcceptThread = null;
        }

        Process.killProcess(Process.myPid());
    }

    @Override
    protected void onActivityResult(int request, int result, Intent intent) {
        if (request == REQUEST_ENABLE_BT && result == RESULT_OK) {
            onBonding();
        }
    }

    private void onBonding() {
        Set<BluetoothDevice> devices = adapter.getBondedDevices();

        if (devices.size() <= 0) {
            return;
        }

        if (mAcceptThread != null) {
            mAcceptThread.cancel();
            mAcceptThread = null;
        }

        mAcceptThread = new AcceptThread(adapter);
        mAcceptThread.start();
    }

    private class AcceptThread extends Thread {

        private BluetoothServerSocket server;

        public AcceptThread(BluetoothAdapter adapter) {
            super("accept");

            try {
                server = adapter.listenUsingRfcommWithServiceRecord("RFCOMM Service", SampleActivity.MY_UUID);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

        @Override
        public void run() {
            boolean isRunnable = true;

            while (isRunnable) {
                try {
                    BluetoothSocket socket = server.accept();

                    if (socket != null) {
                        managedConnection(socket);
                    }
                } catch (IOException e) {
                    Log.v(TAG, "disconnect", e);

                    isRunnable = false;

                    cancel();
                }
            }
        }

        private void managedConnection(final BluetoothSocket socket) {
            handler.post(new Thread() {
                @Override
                public void run() {
                    BluetoothDevice device = socket.getRemoteDevice();

                    Toast.makeText(
                            SampleActivity.this,
                            String.format("Connect: %s:%s" ,device.getName(), device.getAddress()),
                            Toast.LENGTH_LONG
                    ).show();
                }
            });

            handler.obtainMessage(SampleActivity.MESSAGE_CLIENT_ACCEPT, socket).sendToTarget();
        }

        public void cancel() {
            try {
                server.close();
                server = null;
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    private class ConnectThread extends Thread {

        private BluetoothSocket socket;

        public ConnectThread(BluetoothSocket socket) {
            super("connect");

            this.socket = socket;
        }

        @Override
        public void run() {
            InputStream is = null;

            try {
                is = socket.getInputStream();

                try {
                    BufferedReader br = new BufferedReader(new InputStreamReader(is));
                    String str = null;

                    while ((str = br.readLine()) != null) {
                        handler.obtainMessage(SampleActivity.MESSAGE_RECEIVE, str).sendToTarget();
                    }
                } catch (IOException  e) {
                    Log.v(TAG, "disconnect", e);

                    cancel();
                } finally {
                    if (is != null) {
                        try {
                            is.close();
                        } catch (IOException e) {
                            e.printStackTrace();
                        }
                    }
                }
            } catch (IOException e) {
                e.printStackTrace();
            } finally {
                if(is != null) {
                    try {
                        is.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
            }
        }

        public void cancel() {
            if(socket != null) {
                try {
                    socket.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
    }

}