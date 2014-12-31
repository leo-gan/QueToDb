Manual Installation:
* create the ServiceBus namespace
* create the EventHup
* create the Send and Listen policies
* copy the policies from the EventHup configuration page into the config files of the sender and receiver applications
* create the BLOB storage
* copy the storage connnection string into the config file of receiver application

Testing:
* make sure the sender and receiver are in synch. Receiver does not set up the offsets, so receiver should receive all messages before testing.
* now use DebugView to see the send and receive messages and compare them. 