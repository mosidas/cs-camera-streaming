import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { CameraFeed } from "./components/CameraFeed";
import { Home } from "./components/Home";

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/fetch-data',
    element: <FetchData />
  },
  {
    path: '/camera-feed',
    element: <CameraFeed />
  }
];

export default AppRoutes;
