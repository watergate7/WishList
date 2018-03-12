import React, { Component } from 'react';
import { Route } from 'react-router';
import { HashRouter } from 'react-router-dom';
import './App.css';
import AppNavbar from './components/Navbar';
import WishGallery from './components/WishGallery';
import MakeWish from './components/MakeWish';

class App extends Component {
  render() {
    return (
      <HashRouter>
        <div className="App">
          <div>
            <AppNavbar />
          </div>
          <div className="App-body">
            <Route exact path="/" component={WishGallery} />
            <Route path="/makewish" component={MakeWish} />
          </div>
        </div>
      </HashRouter>
    );
  }
}

export default App;
