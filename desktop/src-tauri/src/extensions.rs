use tokio::sync::{mpsc, watch};
use tokio_stream::wrappers::{ReceiverStream, WatchStream};

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
