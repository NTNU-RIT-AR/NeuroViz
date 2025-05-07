use tokio::sync::{mpsc, watch};
use tokio_stream::wrappers::{ReceiverStream, WatchStream};

/// Helper extension to convert watch receiver into a stream
pub trait WatchReceiverExt<T> {
    fn into_stream(self) -> WatchStream<T>
    where
        Self: Sized;
}

impl<T: Clone + Send + Sync + 'static> WatchReceiverExt<T> for watch::Receiver<T> {
    fn into_stream(self) -> WatchStream<T> {
        WatchStream::new(self)
    }
}

/// Helper extension to convert mpsc receiver into a stream
pub trait MpscReceiverExt<T> {
    fn into_stream(self) -> ReceiverStream<T>
    where
        Self: Sized;
}

impl<T> MpscReceiverExt<T> for mpsc::Receiver<T> {
    fn into_stream(self) -> ReceiverStream<T> {
        ReceiverStream::new(self)
    }
}

pub trait WatchSenderExt<T> {
    fn send_modify_with<F, V>(&self, modify: F) -> V
    where
        F: FnOnce(&mut T) -> V;
}

impl<T> WatchSenderExt<T> for watch::Sender<T> {
    fn send_modify_with<F, V>(&self, modify: F) -> V
    where
        F: FnOnce(&mut T) -> V,
    {
        let mut value = None;

        self.send_modify(|state| {
            value = Some(modify(state));
        });

        value.expect("Failed to modify state")
    }
}
