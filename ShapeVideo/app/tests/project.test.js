import React from 'react';
import { shallow } from 'enzyme';
import ShapeVideo from '../ShapeVideo';

let sv = null;
const events = {};

beforeEach(() => {
  document.addEventListener = jest.fn((event, cb) => {
    events[event] = cb;
  });
  sv = shallow(<ShapeVideo />);
});

it('toggles with handleConfirmToggle', () => {
  expect(sv.state().isConfirmOpen).toBe(false);
  sv.instance().handleConfirmToggle();
  expect(sv.state().isConfirmOpen).toBe(true);
  sv.instance().handleConfirmToggle();
  expect(sv.state().isConfirmOpen).toBe(false);
  sv.instance().handleConfirmToggle();
  expect(sv.state().isConfirmOpen).toBe(true);
});

it('stays open on ctrl+n', () => {
  events.keydown({ key: 'n', ctrlKey: true });
  expect(sv.state().isConfirmOpen).toBe(true);
  events.keydown({ key: 'n', ctrlKey: true });
  expect(sv.state().isConfirmOpen).toBe(true);
  events.keydown({ key: 'n', ctrlKey: true });
  expect(sv.state().isConfirmOpen).toBe(true);
});

it('new project resets values', () => {
  sv.instance().handleNewProject();
  expect(sv.state().project).toMatchObject({
    frames: [],
    name: '',
    videoFilePath: ''
  });
});

it('save and open new project', () => {
  sv.instance().handleNewProject();
  sv.instance().handleSaveProject('save and open new project.sheo');
  sv.instance().handleOpenProject('save and open new project.sheo');
  expect(sv.state().project).toMatchObject({
    frames: [],
    name: '',
    videoFilePath: ''
  });
});
