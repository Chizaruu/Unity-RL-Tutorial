public interface IState<T> {
  T SaveState();
  void LoadState(T state);
}