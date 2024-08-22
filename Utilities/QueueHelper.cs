using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class QueueHelper
    {
        private MessageQueue _messageQueue;
        public const int MESSAGE_TYPE = 1;

        public QueueHelper(string queueName)
        {
            if (MessageQueue.Exists(queueName))
                //creates an instance MessageQueue, which points 
                //to the already existing MyQueue
                _messageQueue = new MessageQueue(queueName);
            else
                //creates a new queue 
                _messageQueue = MessageQueue.Create(queueName);
            _messageQueue.MessageReadPropertyFilter.AppSpecific = true;
            _messageQueue.Formatter = new BinaryMessageFormatter();
        }

        public bool IsQueueEmpty()
        {
            bool isQueueEmpty = false;
            try
            {
                // Set Peek to return immediately.
                _messageQueue.Peek(new TimeSpan(0));

                // If an IOTimeout was not thrown, there is a message 
                // in the queue.
                isQueueEmpty = false;
            }

            catch (MessageQueueException e)
            {
                if (e.MessageQueueErrorCode ==
                    MessageQueueErrorCode.IOTimeout)
                {
                    // No message was in the queue.
                    isQueueEmpty = true;
                }

                // Handle other sources of MessageQueueException.
            }
            // Handle other exceptions as necessary.

            // Return true if there are no messages in the queue.
            return isQueueEmpty;

        }

        public object Peek()
        {
            try
            {
                // time-out 30 seconds
                Message msg = _messageQueue.Peek(new TimeSpan(0, 0, 30));
                return msg.Body;
            }
            catch
            {
                return null;
            }
        }

        public object Receive()
        {
            try
            {
                // time-out 30 seconds
                Message msg = _messageQueue.Receive(new TimeSpan(0, 0, 30));
                return msg.Body;
            }
            catch
            {
                return null;
            }
        }

        public void Send(object data)
        {
            Message msg = new Message(data, new BinaryMessageFormatter());
            msg.AppSpecific = MESSAGE_TYPE;
            _messageQueue.Send(data);
        }
    }
}
